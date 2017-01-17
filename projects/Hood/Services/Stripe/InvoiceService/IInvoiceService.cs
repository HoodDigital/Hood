using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Services
{
    /// <summary>
    /// Interface for loading and communicating with Stripe Invoices
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Finds the invoice by identifier.
        /// </summary>
        /// <param name="invoiceId">The stripe subscription invoice identifier.</param>
        /// <returns></returns>
        Task<StripeInvoice> FindByIdAsync(string invoiceId);

        /// <summary>
        /// Gets the next upcoming invoice for the given customer.
        /// </summary>
        /// <param name="customerId">The customer's stripe id.</param>
        /// <returns></returns>
        Task<StripeInvoice> GetUpcoming(string customerId);

        /// <summary>
        /// Returns all the subscription invoices for this account.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<StripeInvoice>> GetAllAsync(string customerId, string startAfterId, int? pageSize = null);

    }
}
