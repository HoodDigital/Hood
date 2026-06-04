using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// Exercises the flagged view-navigation queries (EF8+ risk) against a real database: the
    /// ContentView / PropertyListingView multi-collection .Include chains and the UserProfileView
    /// query. Asserts they translate and execute without error (an empty result is fine). Skips when
    /// no database is available.
    /// </summary>
    [Collection("Database")]
    public class ViewQueryTests
    {
        private readonly DatabaseFixture _db;

        public ViewQueryTests(DatabaseFixture db) => _db = db;

        [SkippableFact]
        public void ContentView_with_navigation_includes_executes()
        {
            Skip.IfNot(_db.Available, _db.UnavailableReason);

            using var ctx = _db.NewContent();
            var rows = ctx
                .ContentViews.Include(c => c.Metadata)
                .Include(c => c.Categories)
                .Include(c => c.Media)
                .Take(5)
                .ToList();

            Assert.NotNull(rows);
        }

        [SkippableFact]
        public void PropertyListingView_with_navigation_includes_executes()
        {
            Skip.IfNot(_db.Available, _db.UnavailableReason);

            using var ctx = _db.NewProperty();
            var rows = ctx
                .PropertyViews.Include(p => p.Metadata)
                .Include(p => p.Media)
                .Include(p => p.FloorPlans)
                .Take(5)
                .ToList();

            Assert.NotNull(rows);
        }

        [SkippableFact]
        public void UserProfileView_executes()
        {
            Skip.IfNot(_db.Available, _db.UnavailableReason);

            using var ctx = _db.NewIdentity();
            var rows = ctx.UserProfileViews.Take(5).ToList();

            Assert.NotNull(rows);
        }
    }
}
