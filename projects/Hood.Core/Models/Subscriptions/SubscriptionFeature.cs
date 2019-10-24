using Newtonsoft.Json;

namespace Hood.Models
{
    public partial class SubscriptionFeature : MetadataBase, IMetadata
    {
        public SubscriptionFeature()
        {
        }

        public SubscriptionFeature(string name, string value, string type = "System.String") : base(name, value, type)
        {
        }

        [JsonIgnore]
        public int SubscriptionId { get; set; }
        [JsonIgnore]
        public Subscription Subscription { get; set; }
    }

}

