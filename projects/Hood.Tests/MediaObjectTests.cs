using Hood.Models;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// Pure unit tests for the <see cref="MediaBase"/> convenience constructor's URL fallback
    /// logic (HOOD-80). No database — exercises the ctor in isolation.
    /// </summary>
    public class MediaObjectTests
    {
        [Fact]
        public void Ctor_with_only_url_falls_all_sizes_back_to_url()
        {
            var media = new MediaObject("https://cdn/full.jpg");

            Assert.Equal("https://cdn/full.jpg", media.Url);
            Assert.Equal("https://cdn/full.jpg", media.ThumbUrl);
            Assert.Equal("https://cdn/full.jpg", media.SmallUrl);
            Assert.Equal("https://cdn/full.jpg", media.MediumUrl);
            Assert.Equal("https://cdn/full.jpg", media.LargeUrl);
        }

        [Fact]
        public void Ctor_assigns_each_size_url_to_its_own_field()
        {
            var media = new MediaObject(
                url: "https://cdn/full.jpg",
                smallUrl: "https://cdn/small.jpg",
                mediumUrl: "https://cdn/medium.jpg",
                largeUrl: "https://cdn/large.jpg",
                thumbUrl: "https://cdn/thumb.jpg"
            );

            Assert.Equal("https://cdn/full.jpg", media.Url);
            Assert.Equal("https://cdn/thumb.jpg", media.ThumbUrl);
            Assert.Equal("https://cdn/small.jpg", media.SmallUrl);
            Assert.Equal("https://cdn/medium.jpg", media.MediumUrl);
            Assert.Equal("https://cdn/large.jpg", media.LargeUrl);
        }

        [Fact]
        public void Ctor_falls_back_per_field_when_only_some_sizes_set()
        {
            // Only medium supplied — small/large/thumb fall back to url, medium keeps its own.
            var media = new MediaObject(
                url: "https://cdn/full.jpg",
                mediumUrl: "https://cdn/medium.jpg"
            );

            Assert.Equal("https://cdn/medium.jpg", media.MediumUrl);
            Assert.Equal("https://cdn/full.jpg", media.SmallUrl);
            Assert.Equal("https://cdn/full.jpg", media.LargeUrl);
            Assert.Equal("https://cdn/full.jpg", media.ThumbUrl);
        }
    }
}
