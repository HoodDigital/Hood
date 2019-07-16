using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using Microsoft.AspNetCore.Identity;
using Hood.Models;

namespace Hood.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IStripeService _stripe;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvoiceService(IStripeService stripe,
                               UserManager<ApplicationUser> userManager)
        {
            _stripe = stripe;
            _userManager = userManager;
        }

        public async Task<Stripe.Invoice> FindByIdAsync(string invoiceId)
        {
            Stripe.Invoice response = await _stripe.InvoiceService.GetAsync(invoiceId);
            return response;
        }

        public async Task<IEnumerable<Stripe.Invoice>> GetAllAsync(string customerId, string startAfterId, int? pageSize = null)
        {
            Stripe.InvoiceListOptions options = new Stripe.InvoiceListOptions()
            {
                CustomerId = customerId,
                StartingAfter = startAfterId,
                Limit = pageSize
            };
            IEnumerable<Stripe.Invoice> response = await _stripe.InvoiceService.ListAsync(options);
            return response;
        }

        public async Task<Stripe.Invoice> GetUpcoming(string customerId)
        {
            Stripe.Invoice response = await _stripe.InvoiceService.UpcomingAsync(new UpcomingInvoiceOptions() { CustomerId = customerId });
            return response;
        }
    }
}
