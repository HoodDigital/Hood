using Hood.Interfaces;

namespace Hood.Models
{
    public partial class PropertyMedia : MediaBase
    {
        public PropertyListing Property { get; internal set; }
        public int PropertyId { get; internal set; }

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
        public PropertyListing Property { get; internal set; }
        public int PropertyId { get; internal set; }

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
