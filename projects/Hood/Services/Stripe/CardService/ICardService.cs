using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Services
{
    /// <summary>
    /// Interface for loading and communicating with Stripe Cards
    /// </summary>
    public interface ICardService
    {
        /// <summary>
        /// Finds the card by identifier.
        /// </summary>
        /// <param name="cardId">The stripe subscription card identifier.</param>
        /// <returns></returns>
        Task<Stripe.Card> FindByIdAsync(string customerId, string cardId);

        /// <summary>
        /// Returns all the subscription cards for this account.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Stripe.Card>> GetAllAsync(string customerId);

        /// <summary>
        /// This creates a new card on stripe for the customer.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Stripe.Card> CreateCard(string customerId, string token);

        /// <summary>
        /// Deletes the card asynchronous.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="cardId">The card identifier.</param>
        /// <returns></returns>
        Task DeleteCard(string customerId, string cardId);
    }
}
