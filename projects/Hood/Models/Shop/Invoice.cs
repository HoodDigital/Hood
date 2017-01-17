using Stripe;
namespace Hood.Models
{
    public class Invoice
    {
        public StripeInvoice StripeInvoice { get; set; }

        public string AmountDue
        {
            get
            {
                return ((double)StripeInvoice.AmountDue / 100).ToString("C");
            }
        }

        public Invoice(StripeInvoice invoice)
        {
            StripeInvoice = invoice;
        }
    }

}