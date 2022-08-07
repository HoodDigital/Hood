

using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

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
        [NotMapped]
        public PropertyListingView PropertyListingView { get; set; }

    }
}

