using Hood.Interfaces;
using System;

namespace Hood.Models
{
    public partial class PropertyMedia : PropertyMediaBase
    {
        public PropertyListing Property { get; internal set; }
        public int PropertyId { get; internal set; }

        public PropertyMedia()
            : base()
        {}

        public PropertyMedia(IMediaObject media)
            : base(media)
        {}
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
    }

    public partial class PropertyMediaBase : IMediaObject
    {
        public PropertyMediaBase()
        {
        }

        public PropertyMediaBase(IMediaObject media)
        {
            this.FileSize = media.FileSize;
            this.FileType = media.FileType;
            this.Filename = media.Filename;
            this.Directory = media.Directory;
            this.BlobReference = media.BlobReference;
            this.Url = media.Url;
            this.CreatedOn = media.CreatedOn;
            this.GeneralFileType = media.GeneralFileType;
            this.ThumbUrl = media.ThumbUrl;
            this.SmallUrl = media.SmallUrl;
            this.MediumUrl = media.MediumUrl;
            this.LargeUrl = media.LargeUrl;
            this.UniqueId = media.UniqueId;
        }

        public int Id { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
        public string Filename { get; set; }
        public string Directory { get; set; }
        public string BlobReference { get; set; }
        public string Url { get; set; }
        public DateTime CreatedOn { get; set; }
        public string GeneralFileType { get; set; }
        public string ThumbUrl { get; set; }
        public string SmallUrl { get; set; }
        public string MediumUrl { get; set; }
        public string LargeUrl { get; set; }
        public string UniqueId { get; set; }

    }
}
