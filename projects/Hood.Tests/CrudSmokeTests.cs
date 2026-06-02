using System;
using System.Linq;
using Hood.Enums;
using Hood.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// Data-layer CRUD smoke against the real v7 schema for the primary standard-Identity paths:
    /// role management (AspNetRoles) and content (HoodContent). Each test runs inside a transaction
    /// that is rolled back on dispose, so nothing is persisted to the target database.
    ///
    /// HTTP-level integration (login/admin render via WebApplicationFactory) is deferred: Hood's
    /// bootstrap runs in Program.Main via LoadHoodAsync(), outside the host pipeline WAF drives —
    /// exercising it needs a test-host refactor (tracked separately).
    /// </summary>
    [Collection("Database")]
    public class CrudSmokeTests
    {
        private readonly DatabaseFixture _db;

        public CrudSmokeTests(DatabaseFixture db) => _db = db;

        [SkippableFact]
        public void Role_create_read_delete_round_trips()
        {
            Skip.IfNot(_db.Available, _db.UnavailableReason);

            using var ctx = _db.NewIdentity();
            using var tx = ctx.Database.BeginTransaction(); // rolled back on dispose

            string name = "__smoke_role_" + Guid.NewGuid().ToString("N");
            ctx.Roles.Add(new IdentityRole(name) { NormalizedName = name.ToUpperInvariant() });
            ctx.SaveChanges();

            Assert.NotNull(ctx.Roles.SingleOrDefault(r => r.Name == name));

            ctx.Roles.Remove(ctx.Roles.Single(r => r.Name == name));
            ctx.SaveChanges();

            Assert.Null(ctx.Roles.SingleOrDefault(r => r.Name == name));
        }

        [SkippableFact]
        public void Content_create_read_update_delete_round_trips()
        {
            Skip.IfNot(_db.Available, _db.UnavailableReason);

            using var ctx = _db.NewContent();
            using var tx = ctx.Database.BeginTransaction(); // rolled back on dispose

            var content = new Content
            {
                Title = "Smoke Test Page",
                Excerpt = "smoke",
                Body = "body",
                Slug = "smoke-" + Guid.NewGuid().ToString("N"),
                ContentType = "page",
                Status = ContentStatus.Published,
                PublishDate = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                LastEditedOn = DateTime.UtcNow,
            };

            ctx.Content.Add(content);
            ctx.SaveChanges();
            Assert.True(content.Id > 0);

            var found = ctx.Content.Single(c => c.Id == content.Id);
            found.Title = "Updated Title";
            ctx.SaveChanges();
            Assert.Equal("Updated Title", ctx.Content.AsNoTracking().Single(c => c.Id == content.Id).Title);

            ctx.Content.Remove(found);
            ctx.SaveChanges();
            Assert.Empty(ctx.Content.Where(c => c.Id == content.Id));
        }
    }
}
