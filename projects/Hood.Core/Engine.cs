using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Hood.Caching;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Hood.Core
{
    /// <summary>
    /// Globally accessible class containing static accessors to key areas of the Hood CMS Engine.
    /// </summary>
    public class Engine
    {
        #region Methods

        /// <summary>
        /// Create a static instance of the Hood engine.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IHoodServiceProvider CreateHoodServiceProvider()
        {
            if (Singleton<IHoodServiceProvider>.Instance == null)
                Singleton<IHoodServiceProvider>.Instance = new HoodServiceProvider();

            return Singleton<IHoodServiceProvider>.Instance;
        }

        #endregion

        #region Static Accessors

        /// <summary>
        /// Gets the singleton HoodServiceProvider used to manage and provide global access to Hood services.
        /// </summary>
        public static IHoodServiceProvider Services
        {
            get
            {
                if (Singleton<IHoodServiceProvider>.Instance == null)
                {
                    CreateHoodServiceProvider();
                }

                return Singleton<IHoodServiceProvider>.Instance;
            }
        }

        public static Assembly ResolveUI(string uiName)
        {
            // Register all Hood Components
            var typeFinder = Engine.Services.Resolve<ITypeFinder>();
            var dependencies = typeFinder.FindClassesOfType<IHoodComponent>();

            var instances = dependencies
                .Select(dependencyRegistrar =>
                    (IHoodComponent)Activator.CreateInstance(dependencyRegistrar)
                )
                .OrderBy(dependencyRegistrar => dependencyRegistrar.ServiceConfigurationOrder);

            foreach (var dependency in instances)
            {
                if (dependency.IsUIComponent && dependency.Name == uiName)
                    return dependency.GetType().Assembly;
            }

            return null;
        }

        public static bool Auth0Enabled
        {
            get
            {
                if (Auth0Configuration.ClientId.IsSet() && Auth0Configuration.Domain.IsSet())
                {
                    return true;
                }
                return false;
            }
        }

        public static HoodConfiguration Configuration
        {
            get
            {
                var config = Services.Resolve<IOptions<HoodConfiguration>>();
                if (config != null)
                {
                    return config.Value;
                }
                else
                    return null;
            }
        }

        public static Auth0Configuration Auth0Configuration
        {
            get
            {
                var config = Services.Resolve<IOptions<Auth0Configuration>>();
                if (config != null)
                {
                    return config.Value;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the current resolvable version of the ISettingsRepository.
        /// </summary>
        public static ISettingsRepository Settings
        {
            get { return Services.Resolve<ISettingsRepository>(); }
        }

        /// <summary>
        /// Gets the current resolvable version of the IHoodCache.
        /// </summary>
        public static IHoodCache Cache
        {
            get { return Services.Resolve<IHoodCache>(); }
        }

        /// <summary>
        /// Gets the current resolvable version of the ILogService.
        /// </summary>
        public static ILogService Logs
        {
            get { return Services.Resolve<ILogService>(); }
        }

        /// <summary>
        /// Gets the current user's account, from context, cache or datastore.
        /// </summary>
        public static ClaimsPrincipal Account
        {
            get
            {
                try
                {
                    var _contextAccessor = Services.Resolve<IHttpContextAccessor>();

                    if (
                        _contextAccessor == null
                        || _contextAccessor.HttpContext == null
                        || _contextAccessor.HttpContext.Session == null
                    )
                        return null;

                    return _contextAccessor.HttpContext.User;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the current resolvable version of the IMediaManager service.
        /// </summary>
        public static IMediaManager Media
        {
            get { return Services.Resolve<IMediaManager>(); }
        }

        /// <summary>
        /// Gets the current resolvable version of the IAccountRepository.
        /// </summary>
        public static IThemesService Themes
        {
            get { return Services.Resolve<IThemesService>(); }
        }

        /// <summary>
        /// Gets the current resolvable version of the IEventsService.
        /// </summary>
        public static IEventsService Events
        {
            get { return Services.Resolve<IEventsService>(); }
        }

        /// <summary>
        /// Resolves the URL for a Hood CMS asset, in this resolution order:
        /// <list type="number">
        /// <item><c>BypassCDN: true</c> — returns <paramref name="localPath"/> to serve from the app's own <c>wwwroot</c>.</item>
        /// <item><c>CdnFullPath</c> set — returns <c>{CdnFullPath}{localPath}</c> verbatim; Hood appends no version, so the consumer owns the version pin (use for self-hosting, mirrors, or pinning a specific version).</item>
        /// <item>Default — returns <c>{CdnPath}@{ResourceVersion}{localPath}</c>, with <see cref="ResourceVersion"/> carrying the prerelease tag so rc builds resolve on jsDelivr.</item>
        /// </list>
        /// Note: <c>asp-append-version</c> is a silent no-op on these CDN URLs — the tag helper can only hash local
        /// <c>wwwroot</c> files, not remote CDN URLs, so it must not be relied on for cache-busting Hood resources.
        /// Content-hashed cache-busting is not applied to these CDN URLs.
        /// </summary>
        public static string Resource(string localPath)
        {
            if (Configuration.BypassCDN)
            {
                return localPath;
            }
            if (CdnFullPath.IsSet())
            {
                return $"{CdnFullPath}{localPath}";
            }
            return $"{CdnPath}@{ResourceVersion}{localPath}";
        }

        public static string CdnPath
        {
            get
            {
                try
                {
                    if (Configuration.CdnPath.IsSet())
                    {
                        return Configuration.CdnPath;
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause — config may be unavailable
                // during startup, and logging from inside the engine here would recurse.
                catch (Exception) { }
                return "https://cdn.jsdelivr.net/npm/hoodcms";
            }
        }

        /// <summary>
        /// Optional complete CDN base URL used verbatim by <see cref="Resource(string)"/> — Hood appends no
        /// <c>@{version}</c> segment, so the consumer owns the version pin. Empty unless the consumer sets
        /// <c>Hood:CdnFullPath</c>. Use for self-hosting/mirroring Hood assets or pinning a specific version
        /// without overriding the <c>_Scripts</c>/<c>_Styles</c> views.
        /// </summary>
        public static string CdnFullPath
        {
            get
            {
                try
                {
                    if (Configuration.CdnFullPath.IsSet())
                    {
                        return Configuration.CdnFullPath;
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause — config may be unavailable
                // during startup, and logging from inside the engine here would recurse.
                catch (Exception) { }
                return null;
            }
        }

        public static string SiteOwnerEmail
        {
            get
            {
                if (Configuration != null)
                {
                    return Configuration.SuperAdminEmail;
                }
                return "admin@hooddigital.com";
            }
        }

        public static string Version
        {
            get
            {
                var version = typeof(Engine).Assembly.GetName().Version;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        /// <summary>
        /// The version used to compose CDN asset URLs. Reads <c>AssemblyInformationalVersion</c> (truncated of
        /// any <c>+build</c> metadata) so prerelease builds carry the <c>-rc.N</c> tag — without it, rc consumers
        /// request <c>hoodcms@7.0.0</c> from jsDelivr while npm only has <c>7.0.0-rc.N</c>, 404-ing the admin UI.
        /// Falls back to <see cref="Version"/> (<c>Major.Minor.Build</c>) if the informational version is absent.
        /// Distinct from <see cref="Version"/>, which stays clean for footers, the <c>/version</c> endpoint and the
        /// persisted <c>Hood.Version</c> DB marker.
        /// </summary>
        public static string ResourceVersion
        {
            get
            {
                var informational = typeof(Engine)
                    .Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion;
                return informational.IsSet() ? StripBuildMetadata(informational) : Version;
            }
        }

        /// <summary>
        /// Strips the <c>+{build}</c> metadata SemVer appends to an informational version, keeping any
        /// <c>-rc.N</c> prerelease tag (e.g. <c>7.0.0-rc.24+abc123 → 7.0.0-rc.24</c>). Returns the input
        /// unchanged when there is no build metadata.
        /// </summary>
        internal static string StripBuildMetadata(string informationalVersion)
        {
            if (!informationalVersion.IsSet())
            {
                return informationalVersion;
            }
            var plus = informationalVersion.IndexOf('+');
            return plus >= 0 ? informationalVersion.Substring(0, plus) : informationalVersion;
        }

        public static string Url
        {
            get
            {
                if (Settings["Hood.SiteUrl"] != null)
                {
                    return Settings["Hood.SiteUrl"];
                }
                return null;
            }
        }

        public static string ApplicationKey
        {
            get
            {
                if (Settings["Hood.Api.SystemPrivateKey"] != null)
                {
                    return Settings["Hood.Api.SystemPrivateKey"];
                }
                return null;
            }
        }

        #endregion
    }
}
