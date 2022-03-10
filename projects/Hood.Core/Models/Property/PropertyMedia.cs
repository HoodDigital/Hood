using Hood.Interfaces;
using Newtonsoft.Json;

namespace Hood.Models
{
    public partial class PropertyMedia : MediaBase
    {
        [JsonIgnore]
        public PropertyListing Property { get; set; }
        public int PropertyId { get; set; }

        public PropertyMedia()
            : base()
        { }
        public PropertyMedia(IMediaObject media)
            : base(media)
        { }
        public PropertyMedia(string url, string smallUrl = null, string mediumUrl = null, string largeUrl = null, string thumbUrl = null)
        : base(url, smallUrl, mediumUrl, largeUrl, thumbUrl)
        {
        }

        public new static IMediaObject Blank => MediaBase.Blank;
    }

    public partial class PropertyFloorplan : MediaBase
    {
        [JsonIgnore]
        public PropertyListing Property { get; set; }
        public int PropertyId { get; set; }

        public PropertyFloorplan()
            : base()
        { }

        public PropertyFloorplan(IMediaObject media)
            : base(media)
        { }
        public PropertyFloorplan(string url, string smallUrl = null, string mediumUrl = null, string largeUrl = null, string thumbUrl = null)
           : base(url, smallUrl, mediumUrl, largeUrl, thumbUrl)
        {
        }

        public new static IMediaObject Blank => MediaBase.Blank;
    }
}
