using Hood.Interfaces;

namespace Hood.Models
{
    public partial class SubscriptionFeature : IMetadata
    {
        public int Id { get; set; }
        public string BaseValue { get; set; }
        public bool IsStored { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }

    }

}

