using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Hood.Interfaces;
using Newtonsoft.Json;

namespace Hood.Models
{
    public class ContentMedia : MediaBase
    {
        public ContentMedia() { }

        public ContentMedia(IMediaObject mediaResult)
            : base(mediaResult) { }

        public ContentMedia(
            string url,
            string smallUrl = null,
            string mediumUrl = null,
            string largeUrl = null,
            string thumbUrl = null
        )
            : base(url, smallUrl, mediumUrl, largeUrl, thumbUrl) { }

        public int ContentId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public Content Content { get; set; }

        [NotMapped]
        [JsonIgnore]
        [IgnoreDataMember]
        public ContentView ContentView { get; set; }

        public static new IMediaObject Blank => MediaBase.Blank;
    }
}
