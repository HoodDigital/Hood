using System;
using System.Linq;
using Hood.Contexts;
using Hood.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// Auth0 path harness (best-effort, non-gating — no consumer uses Auth0 at runtime). Asserts the
    /// Auth0IdentityContext model validates and maps the Auth0-specific objects: the AspNetAuth0Identities
    /// table and the keyless HoodAuth0UserProfiles view. Model build alone catches EF10 config breaks.
    /// </summary>
    public class Auth0HarnessTests
    {
        private static Auth0IdentityContext NewContext() =>
            new Auth0IdentityContextFactory().CreateDbContext(Array.Empty<string>());

        [Fact]
        public void Auth0_context_maps_the_auth0_identity_table()
        {
            using var ctx = NewContext();

            bool hasAuth0Identities = ctx.Model.GetEntityTypes()
                .Any(e => e.GetTableName() == "AspNetAuth0Identities");

            Assert.True(hasAuth0Identities, "Auth0IdentityContext should map the AspNetAuth0Identities table.");
        }

        [Fact]
        public void Auth0_user_profile_view_is_keyless_and_mapped_to_the_view()
        {
            using var ctx = NewContext();

            var view = ctx.Model.FindEntityType(typeof(UserProfileView<Auth0Role>));

            Assert.NotNull(view);
            Assert.Equal("HoodAuth0UserProfiles", view.GetViewName());
            Assert.Null(view.FindPrimaryKey()); // mapped with HasNoKey()
        }
    }
}
