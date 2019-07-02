using Hood.Infrastructure;
using Hood.Models;

namespace Hood.ViewModels
{
    public class EditSubscriptionModel
    {
        public Subscription Subscription { get; set; }
        public OperationResult SaveResult { get; internal set; }
    }

}