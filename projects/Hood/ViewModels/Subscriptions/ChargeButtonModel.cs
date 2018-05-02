using Hood.Models;

namespace Hood.ViewModels
{
    public class ChargeButtonModel
    {
        public SubscriptionModel SubscriptionModel { get; set; }
        public Subscription Subscription { get; set; }
        public string Text { get; set; }
        public string CssClass { get; set; }
    }
}