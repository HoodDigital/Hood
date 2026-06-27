using Hood.Core;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// CDN asset URLs must carry the prerelease tag. <see cref="Engine.ResourceVersion"/> reads
    /// AssemblyInformationalVersion and strips the <c>+build</c> metadata while keeping any <c>-rc.N</c> tag,
    /// so rc consumers request <c>hoodcms@7.0.0-rc.N</c> (which exists on npm) rather than <c>7.0.0</c> (404).
    /// </summary>
    public class EngineResourceVersionTests
    {
        [Theory]
        [InlineData("7.0.0-rc.24+a6e2131958031874dbe85ef489b78d", "7.0.0-rc.24")] // prerelease + build metadata
        [InlineData("7.0.0+a6e2131958031874dbe85ef489b78d", "7.0.0")] // stable + build metadata
        [InlineData("7.0.0-rc.24", "7.0.0-rc.24")] // prerelease, no metadata
        [InlineData("7.0.0", "7.0.0")] // stable, no metadata
        [InlineData("", "")] // empty passthrough
        [InlineData(null, null)] // null passthrough
        public void StripBuildMetadata_DropsBuildKeepsPrereleaseTag(string input, string expected)
        {
            Assert.Equal(expected, Engine.StripBuildMetadata(input));
        }

        [Fact]
        public void ResourceVersion_NeverCarriesBuildMetadata()
        {
            // Whatever the running assembly's informational version is, the CDN segment must never contain
            // the '+{commit}' metadata (jsDelivr would 404 on it).
            Assert.DoesNotContain("+", Engine.ResourceVersion);
        }
    }
}
