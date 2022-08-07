using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Newtonsoft.Json;

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

        [NotMapped]
        public ContentView ContentView { get; set; }
    }
}

