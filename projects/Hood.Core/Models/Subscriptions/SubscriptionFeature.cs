using Newtonsoft.Json;

namespace Hood.Models
{
    public partial class SubscriptionFeature : MetadataBase, IMetadata
    {
        public SubscriptionFeature()
        {
        }

        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }
    }

}

