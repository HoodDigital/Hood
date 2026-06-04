using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Hood.Models
{
    public class ContentMeta : MetadataBase
    {
        public ContentMeta() { }

        public int ContentId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public Content Content { get; set; }

        [NotMapped]
        [JsonIgnore]
        [IgnoreDataMember]
        public ContentView ContentView { get; set; }
    }
}
