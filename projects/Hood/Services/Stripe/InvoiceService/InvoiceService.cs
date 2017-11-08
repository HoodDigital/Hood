using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using Microsoft.AspNetCore.Identity;
using Hood.Models;

namespace Hood.Services
{
    public class InvoiceService : IInvoiceService
    {
        private IStripeService _stripe;
        private UserManager<HoodIdentityUser> _userManager;

        public InvoiceService(IStripeService stripe,
                               UserManager<HoodIdentityUser> userManager)
        {
            _stripe = stripe;
            _userManager = userManager;
        }

        public async Task<StripeInvoice> FindByIdAsync(string invoiceId)
        {
            StripeInvoice response = await _stripe.InvoiceService.GetAsync(invoiceId);
            return response;
        }

        public async Task<IEnumerable<StripeInvoice>> GetAllAsync(string customerId, string startAfterId, int? pageSize = null)
        {
            StripeInvoiceListOptions options = new StripeInvoiceListOptions()
            {
                CustomerId = customerId,
                StartingAfter = startAfterId,
                Limit = pageSize
            };
            IEnumerable<StripeInvoice> response = await _stripe.InvoiceService.ListAsync(options);
            return response;
        }

        public async Task<StripeInvoice> GetUpcoming(string customerId)
        {
            StripeInvoice response = await _stripe.InvoiceService.UpcomingAsync(customerId);
            return response;
        }
    }
}
