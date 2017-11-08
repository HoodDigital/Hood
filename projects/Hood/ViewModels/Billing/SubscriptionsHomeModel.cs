using Hood.Enums;
using Stripe;
using System.Collections.Generic;

namespace Hood.Models
{
    public partial class BillingHomeModel
    {
        public BillingMessage? Message { get; set; }
        public StripeCustomer Customer { get; set; }
        public HoodIdentityUser User { get; set; }
        public IEnumerable<StripeInvoice> Invoices { get; set; }
        public StripeInvoice NextInvoice { get; set; }
    }
}
