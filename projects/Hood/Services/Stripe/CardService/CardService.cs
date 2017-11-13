using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using Microsoft.AspNetCore.Identity;
using Hood.Models;

namespace Hood.Services
{
    public class CardService : ICardService
    {
        private IStripeService _stripe;
        private UserManager<ApplicationUser> _userManager;

        public CardService(IStripeService stripe,
                           UserManager<ApplicationUser> userManager)
        {
            _stripe = stripe;
            _userManager = userManager;
        }

        public async Task<StripeCard> CreateCard(string customerId, string token)
        {
            var card = new StripeCardCreateOptions()
            {
                SourceToken = token
            };
            StripeCard stripeCard = await _stripe.CardService.CreateAsync(customerId, card); // optional isRecipient

            return stripeCard;
        }

        public async Task DeleteCard(string customerId, string cardId)
        {
            await _stripe.CardService.DeleteAsync(customerId, cardId);
        }

        public async Task<StripeCard> FindByIdAsync(string customerId, string cardId)
        {
            StripeCard stripeCard = await _stripe.CardService.GetAsync(customerId, cardId);
            return stripeCard;
        }

        public async Task<IEnumerable<StripeCard>> GetAllAsync(string customerId)
        {
            IEnumerable<StripeCard> response = await _stripe.CardService.ListAsync(customerId);
            return response;
        }
    }
}
