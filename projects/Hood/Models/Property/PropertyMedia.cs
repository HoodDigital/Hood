using Hood.Interfaces;

namespace Hood.Models
{
    public partial class PropertyMedia : PropertyMediaBase
    {
        public PropertyListing Property { get; internal set; }
        public int PropertyId { get; internal set; }

        public PropertyMedia()
            : base()
        { }

        public PropertyMedia(IMediaObject media)
            : base(media)
        { }
        public new static IMediaObject Blank => MediaObjectBase.Blank;
    }
    public partial class PropertyFloorplan : PropertyMediaBase
    {
        public PropertyListing Property { get; internal set; }
        public int PropertyId { get; internal set; }

        public PropertyFloorplan()
            : base()
        { }

        public PropertyFloorplan(IMediaObject media)
            : base(media)
        { }
        public new static IMediaObject Blank => MediaObjectBase.Blank;
    }

    public partial class PropertyMediaBase : MediaObjectBase
    {
        public PropertyMediaBase()
            : base()
        { }

        public PropertyMediaBase(IMediaObject media)
            : base(media)
        { }

        public new static IMediaObject Blank => MediaObjectBase.Blank;
    }
}
