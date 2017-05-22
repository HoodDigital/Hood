using Hood.Interfaces;

namespace Hood.Extensions
{
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
