using Hood.Infrastructure;

namespace Hood.Models
{
    public class EditSubscriptionModel
    {
        public Subscription<HoodIdentityUser> Subscription { get; set; }
        public OperationResult SaveResult { get; internal set; }
    }

}