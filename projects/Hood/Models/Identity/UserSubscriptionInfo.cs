namespace Hood.Models
{
    public class UserSubscriptionInfo
    {
        public string StripeSubscriptionId { get; set; }
        public string StripeId { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public bool Public { get; set; }
        public int Level { get; set; }
        public bool Addon { get; set; }
    }
}