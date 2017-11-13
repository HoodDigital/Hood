using Hood.Infrastructure;

namespace Hood.Models
{
    public class EditSubscriptionModel
    {
        public Subscription Subscription { get; set; }
        public OperationResult SaveResult { get; internal set; }
    }

}