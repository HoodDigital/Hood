using System;

namespace Hood.Interfaces
{
    public interface IMediaObject
    {
        int Id { get; set; }
        long FileSize { get; set; }
        string FileType { get; set; }
        string Filename { get; set; }
        string Directory { get; set; }
        string BlobReference { get; set; }
        string Url { get; set; }
        DateTime CreatedOn { get; set; }
        string GeneralFileType { get; set; }
        string ThumbUrl { get; set; }
        string SmallUrl { get; set; }
        string MediumUrl { get; set; }
        string LargeUrl { get; set; }
        string UniqueId { get; set; }
    }

    public static class IMediaItemExtensions
    {
        public static IMediaObject UpdateUrls(this IMediaObject me, IMediaObject mediaResult)
        {
            me.LargeUrl = mediaResult.LargeUrl;
            me.SmallUrl = mediaResult.SmallUrl;
            me.MediumUrl = mediaResult.MediumUrl;
            me.ThumbUrl = mediaResult.ThumbUrl;
            me.Url = mediaResult.Url;
            me.UniqueId = mediaResult.UniqueId;
            return me;
        }

    }

}
