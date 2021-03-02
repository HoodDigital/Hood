using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Hood.Models
{
    public partial class ContentMeta : MetadataBase, IMetadata
    {
        public ContentMeta()
        {
        }

        public int ContentId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember] 
        public Content Content { get; set; }

    }
}

