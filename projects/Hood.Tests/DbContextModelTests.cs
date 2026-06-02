using System;
using Hood.Contexts;
using Hood.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// Builds each DbContext's model via its design-time factory and forces OnModelCreating to run.
    /// This is the load-bearing EF10 guard: any model-config break (HasDefaultValueSql sentinel,
    /// HasAlternateKey nullability, shared-table 1:1, view navigations, etc.) throws here.
    /// No live database is required — building the model does not open a connection.
    /// </summary>
    public class DbContextModelTests
    {
        [Theory]
        [InlineData("HoodDbContext")]
        [InlineData("ContentContext")]
        [InlineData("PropertyContext")]
        [InlineData("IdentityContext")]
        [InlineData("Auth0IdentityContext")]
        public void Builds_model_without_error(string contextName)
        {
            using DbContext ctx = Create(contextName);

            var entityTypes = ctx.Model.GetEntityTypes(); // forces full model build

            Assert.NotEmpty(entityTypes);
        }

        private static DbContext Create(string name) => name switch
        {
            "HoodDbContext" => new HoodDbContextFactory().CreateDbContext(Array.Empty<string>()),
            "ContentContext" => new ContentContextFactory().CreateDbContext(Array.Empty<string>()),
            "PropertyContext" => new PropertyContextFactory().CreateDbContext(Array.Empty<string>()),
            "IdentityContext" => new IdentityContextFactory().CreateDbContext(Array.Empty<string>()),
            "Auth0IdentityContext" => new Auth0IdentityContextFactory().CreateDbContext(Array.Empty<string>()),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown context"),
        };
    }
}
