using Hood.Models;
using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Services
{
    /// <summary>
    /// Interface for loading and communicating with Stripe Customers
    /// </summary>
    public interface ICustomerService
    {

        /// <summary>
        /// Finds the customer by identifier.
        /// </summary>
        /// <param name="customerId">The stripe subscription customer identifier.</param>
        /// <returns></returns>
        Task<StripeCustomer> FindByIdAsync(string customerId);

        /// <summary>
        /// Returns all the subscription customers for this account.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<StripeCustomer>> GetAllAsync();

        /// <summary>
        /// This creates a new customer on stripe, and if set, adds them to the provided plan id.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="token"></param>
        /// <param name="planId"></param>
        /// <returns></returns>
        Task<StripeCustomer> CreateCustomer(HoodIdentityUser user, string token, string planId = null);

        /// <summary>
        /// Deletes the customer asynchronous.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        void DeleteCustomer(string customerId);

        /// <summary>
        /// This updates the customer record with the cardId as the new default card.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        Task SetDefaultCard(string customerId, string cardId);
    }
}
