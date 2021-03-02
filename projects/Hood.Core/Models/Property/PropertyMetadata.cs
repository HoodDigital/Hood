using System.Text.Json.Serialization;

namespace Hood.Models
{
    public class PropertyMeta : MetadataBase
    {
        public PropertyMeta()
        {
        }

        public int PropertyId { get; set; }

        [JsonIgnore]
        public PropertyListing Property { get; set; }

    }
}

