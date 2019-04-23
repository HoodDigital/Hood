using Hood.Enums;
using Stripe;
using System.Collections.Generic;

namespace Hood.Models
{
    public partial class BillingHomeModel
    {
        public BillingMessage? Message { get; set; }
        public Stripe.Customer Customer { get; set; }
        public ApplicationUser User { get; set; }
        public IEnumerable<Stripe.Invoice> Invoices { get; set; }
        public Stripe.Invoice NextInvoice { get; set; }
    }
}
