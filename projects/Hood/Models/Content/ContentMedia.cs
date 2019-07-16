using Hood.Interfaces;

namespace Hood.Models
{
    public class ContentMedia : MediaBase
    {
        public ContentMedia() : base()
        { }

        public ContentMedia(IMediaObject mediaResult) : base(mediaResult)
        { }

        public ContentMedia(string url, string smallUrl = null, string mediumUrl = null, string largeUrl = null, string thumbUrl = null)
            : base(url, smallUrl, mediumUrl, largeUrl, thumbUrl)
        {
        }

        public int ContentId { get; set; }
        public Content Content { get; set; }

        public new static IMediaObject Blank => MediaBase.Blank;
    }
}
