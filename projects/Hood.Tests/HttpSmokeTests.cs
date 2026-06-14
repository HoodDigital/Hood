using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hood.Contexts;
using Hood.Models;
using Hood.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// Boots the real Hood web app in-process via <see cref="WebApplicationFactory{TEntryPoint}"/>
    /// against the test SQL database and drives HTTP smoke checks (HOOD-72): the anonymous login page
    /// renders, and after a genuine cookie login the standard-Identity admin pages (the HOOD-66 role
    /// views included) render without 500s.
    ///
    /// Engine initialisation already happens inside the host pipeline (ConfigureHood / UseHood), which
    /// WebApplicationFactory runs — so the WAF-hosted app behaves like the real one. The only piece
    /// Program.Main adds after Build() is the LoadHoodAsync() DB-readiness probe; the factory below
    /// invokes that same step against the test host, so production startup is left untouched.
    ///
    /// SkippableFact-gated on the shared <see cref="DatabaseFixture"/>: runs in CI (DB provisioned),
    /// skips cleanly on a machine without one. Auth0 is a separate backend and is not exercised here.
    /// </summary>
    public class HttpSmokeTests : IClassFixture<HttpSmokeFixture>
    {
        private readonly HttpSmokeFixture _fx;

        public HttpSmokeTests(HttpSmokeFixture fx) => _fx = fx;

        // Secure cookies (SecurePolicy.Always) mean the auth + antiforgery cookies are only issued over
        // HTTPS, so the test client must speak https. No auto-redirect: we assert the login POST's 302
        // (success) vs a 200 form re-render (failure) directly, and check admin pages aren't bounced to login.
        private HttpClient NewClient() =>
            _fx.Factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost"),
                    AllowAutoRedirect = false,
                    HandleCookies = true,
                }
            );

        [SkippableFact]
        public async Task Anonymous_login_page_renders_200()
        {
            Skip.IfNot(_fx.Db.Available, _fx.Db.UnavailableReason);

            using var client = NewClient();
            var resp = await client.GetAsync("/account/login");

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [SkippableFact]
        public async Task Authenticated_admin_pages_render_without_500()
        {
            Skip.IfNot(_fx.Db.Available, _fx.Db.UnavailableReason);

            using var client = NewClient();

            // GET the login page; the antiforgery cookie is captured by the client's cookie handler.
            var loginGet = await client.GetAsync("/account/login");
            loginGet.EnsureSuccessStatusCode();

            // Replay every hidden field the form rendered — the antiforgery token plus the anti-spam
            // fields (ts/hsh/slt) that LoginViewModel's SpamPreventionModel validation requires — then
            // add the credentials. (The honeypot is a CSS-hidden text input, not type=hidden, so it's
            // correctly left empty.)
            var fields = ExtractHiddenFields(await loginGet.Content.ReadAsStringAsync());
            Skip.IfNot(
                fields.ContainsKey("__RequestVerificationToken"),
                "Antiforgery token not found on the login page."
            );
            fields["Username"] = HttpSmokeFixture.AdminEmail;
            fields["Password"] = HttpSmokeFixture.AdminPassword;
            fields["RememberMe"] = "false";

            var loginPost = await client.PostAsync(
                "/account/login",
                new FormUrlEncodedContent(fields)
            );

            // Success redirects (302) to a local page; a 200 means the form re-rendered = login failed.
            Assert.True(
                loginPost.StatusCode == HttpStatusCode.Redirect
                    || loginPost.StatusCode == HttpStatusCode.Found,
                $"Login POST expected a 302 redirect on success but got {(int)loginPost.StatusCode} {loginPost.StatusCode}."
            );

            // The authenticated admin surfaces must render — no 500, and not bounced (302) back to login.
            foreach (
                var path in new[] { "/admin/users/", "/admin/roles/", "/admin/content/manage/" }
            )
            {
                var resp = await client.GetAsync(path);
                Assert.True(
                    resp.StatusCode == HttpStatusCode.OK,
                    $"GET {path} expected 200 but got {(int)resp.StatusCode} {resp.StatusCode}."
                );
            }
        }

        // Pulls every <input type=hidden> name/value pair out of the rendered page. Handles both
        // single- and double-quoted attributes — the framework antiforgery token uses double quotes,
        // but Hood's honeypot/anti-spam fields (ts/hsh/slt) are rendered single-quoted.
        private static Dictionary<string, string> ExtractHiddenFields(string html)
        {
            var fields = new Dictionary<string, string>();
            foreach (Match tag in Regex.Matches(html, "<input\\b[^>]*>"))
            {
                if (!Regex.IsMatch(tag.Value, "type=[\"']hidden[\"']"))
                    continue;
                var name = Regex.Match(tag.Value, "name=[\"']([^\"']+)[\"']");
                if (!name.Success)
                    continue;
                var value = Regex.Match(tag.Value, "value=[\"']([^\"']*)[\"']");
                fields[name.Groups[1].Value] = value.Success ? value.Groups[1].Value : "";
            }
            return fields;
        }
    }

    /// <summary>
    /// Owns the in-process web host and seeds a standard-Identity admin user (with the Admin role) once
    /// per test class. No-ops when the database is unavailable so the tests skip rather than fail.
    /// </summary>
    public class HttpSmokeFixture : IAsyncLifetime
    {
        public const string AdminEmail = "smoke-admin@hood.test";
        public const string AdminPassword = "Smoke!admin123";

        public DatabaseFixture Db { get; } = new DatabaseFixture();
        public HoodWebApplicationFactory Factory { get; private set; }

        public async Task InitializeAsync()
        {
            if (!Db.Available)
                return;

            Factory = new HoodWebApplicationFactory(Db.ConnectionString);

            // Resolving Services forces the host to build (and runs LoadHoodAsync via the factory).
            using var scope = Factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<
                UserManager<ApplicationUser>
            >();

            foreach (var role in new[] { "SuperUser", "Admin" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var user = await userManager.FindByNameAsync(AdminEmail);
            if (user == null)
            {
                // ApplicationUser and UserProfile table-split the AspNetUsers row sharing a PK, so the
                // profile must be seeded inline with the same Id (mirrors InstallController) — otherwise
                // the row is malformed and the cookie OnValidatePrincipal hook 500s every admin request.
                var id = Guid.NewGuid().ToString();
                user = new ApplicationUser
                {
                    Id = id,
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    EmailConfirmed = true,
                    Active = true,
                    CreatedOn = DateTime.UtcNow,
                    LastLogOn = DateTime.UtcNow,
                    LastLoginIP = "127.0.0.1",
                    LastLoginLocation = "Test",
                    UserProfile = new UserProfile
                    {
                        Id = id,
                        Email = AdminEmail,
                        UserName = AdminEmail,
                        FirstName = "Smoke",
                        LastName = "Admin",
                        JobTitle = "Website Administrator",
                        Anonymous = false,
                    },
                };
                await userManager.CreateAsync(user, AdminPassword);
            }

            if (!await userManager.IsInRoleAsync(user, "Admin"))
                await userManager.AddToRoleAsync(user, "Admin");

            // Run the same install seed production uses: it writes the Hood.Settings.SiteOwner option
            // (which the [Installed] filter gates on, otherwise every page 302s to the install wizard)
            // and seeds the default settings/media directories. Without the seeded IntegrationSettings,
            // the recaptcha tag helper on the login page NREs. GetSiteAdmin reuses our admin because
            // Hood:SuperAdminEmail is pointed at it above. Seed is idempotent, so it's safe to run every
            // time (and repairs a half-installed database, not just a pristine one).
            var hoodDb = scope.ServiceProvider.GetRequiredService<HoodDbContext>();
            var identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
            await hoodDb.Seed(identityContext);
        }

        public Task DisposeAsync()
        {
            Factory?.Dispose();
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// WebApplicationFactory that points the host at the test database and invokes the same
    /// post-build Hood bootstrap (LoadHoodAsync) that Program.Main runs — without altering production
    /// startup.
    /// </summary>
    public class HoodWebApplicationFactory : WebApplicationFactory<Web.Program>
    {
        private readonly string _connectionString;

        public HoodWebApplicationFactory(string connectionString) =>
            _connectionString = connectionString;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration(
                (_, config) =>
                    config.AddInMemoryCollection(
                        new Dictionary<string, string>
                        {
                            ["ConnectionStrings:DefaultConnection"] = _connectionString,
                            // The install seed resolves the site owner via Hood:SuperAdminEmail; point it
                            // at the seeded admin so Seed() reuses that user rather than creating a stray one.
                            ["Hood:SuperAdminEmail"] = HttpSmokeFixture.AdminEmail,
                        }
                    )
            );
            // TestServer leaves Connection.RemoteIpAddress null, but the login action stamps
            // RemoteIpAddress.ToString() onto the user — which would NRE. Real Kestrel always sets it;
            // give the test host a loopback address so the production code path runs unchanged.
            builder.ConfigureServices(services =>
                services.AddSingleton<IStartupFilter, LoopbackRemoteIpStartupFilter>()
            );
        }

        private sealed class LoopbackRemoteIpStartupFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
                app =>
                {
                    app.Use(
                        async (ctx, nextMiddleware) =>
                        {
                            ctx.Connection.RemoteIpAddress ??= IPAddress.Loopback;
                            ctx.Connection.LocalIpAddress ??= IPAddress.Loopback;
                            await nextMiddleware();
                        }
                    );
                    next(app);
                };
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            host.LoadHoodAsync().GetAwaiter().GetResult();
            return host;
        }
    }
}
