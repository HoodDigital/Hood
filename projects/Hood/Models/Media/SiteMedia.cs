using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using System;

namespace Hood.Models
{
    public partial class SiteMedia : BaseEntity, IMediaObject
    {
        public SiteMedia()
        {
        }

        public SiteMedia(IMediaObject mediaResult)
        {
            mediaResult.CopyProperties(this);
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
