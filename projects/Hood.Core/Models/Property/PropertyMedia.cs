using System.ComponentModel.DataAnnotations.Schema;
using Hood.Interfaces;
using Newtonsoft.Json;

namespace Hood.Models
{
    public class PropertyMedia : MediaBase
    {
        [JsonIgnore]
        public PropertyListing Property { get; set; }

        [NotMapped]
        public PropertyListingView PropertyListingView { get; set; }
        public int PropertyId { get; set; }

        public PropertyMedia() { }

        public PropertyMedia(IMediaObject media)
            : base(media) { }

        public PropertyMedia(
            string url,
            string smallUrl = null,
            string mediumUrl = null,
            string largeUrl = null,
            string thumbUrl = null
        )
            : base(url, smallUrl, mediumUrl, largeUrl, thumbUrl) { }

        public static new IMediaObject Blank => MediaBase.Blank;
    }

    public class PropertyFloorplan : MediaBase
    {
        [JsonIgnore]
        public PropertyListing Property { get; set; }

        [NotMapped]
        public PropertyListingView PropertyListingView { get; set; }
        public int PropertyId { get; set; }

        public PropertyFloorplan() { }

        public PropertyFloorplan(IMediaObject media)
            : base(media) { }

        public PropertyFloorplan(
            string url,
            string smallUrl = null,
            string mediumUrl = null,
            string largeUrl = null,
            string thumbUrl = null
        )
            : base(url, smallUrl, mediumUrl, largeUrl, thumbUrl) { }

        public static new IMediaObject Blank => MediaBase.Blank;
    }
}
