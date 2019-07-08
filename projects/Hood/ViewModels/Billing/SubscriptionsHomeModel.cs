using Hood.Enums;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public partial class BillingHomeModel
    {
        public Stripe.Customer Customer { get; set; }
        public IEnumerable<Stripe.Invoice> Invoices { get; set; }
        public Stripe.Invoice NextInvoice { get; set; }
    }
}
