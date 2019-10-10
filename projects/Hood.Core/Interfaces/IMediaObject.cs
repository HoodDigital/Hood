using Hood.Enums;
using System;

namespace Hood.Interfaces
{
    public interface IMediaObject
    {
        int Id { get; set; }
        long FileSize { get; set; }
        string FileType { get; set; }
        string Filename { get; set; }
        string BlobReference { get; set; }
        string Url { get; set; }
        DateTime CreatedOn { get; set; }
        string ThumbUrl { get; set; }
        string SmallUrl { get; set; }
        string MediumUrl { get; set; }
        string LargeUrl { get; set; }
        string UniqueId { get; set; }
        GenericFileType GenericFileType { get; set; }

        string DownloadUrl { get; }
        string DownloadUrlHttps { get; }

        string Icon { get; }
        string FormattedSize { get; }
        string Path { get; }
    }

}
