using System.Collections.Generic;
using System.Threading.Tasks;
using Hood.Models;
using Microsoft.AspNetCore.Identity;
using Hood.Caching;

namespace Hood.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IStripeService _stripe;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHoodCache _cache;

        public CustomerService(IStripeService stripe,
                               IHoodCache cache,
                               UserManager<ApplicationUser> userManager)
        {
            _cache = cache;
            _stripe = stripe;
            _userManager = userManager;
        }

        public async Task<Stripe.Customer> CreateCustomer(ApplicationUser user, string token, string planId = null)
        {
            var customer = new Stripe.CustomerCreateOptions()
            {
                Email = user.Email,
                Description = string.Format("{0} {1} ({2})", user.FirstName, user.LastName, user.Email),
                SourceToken = token,
                PlanId = planId
            };
            Stripe.Customer stripeCustomer = await _stripe.CustomerService.CreateAsync(customer);
            return stripeCustomer;
        }


        public void DeleteCustomer(string customerId)
        {
            _stripe.CustomerService.Delete(customerId);
        }

        public async Task<Stripe.Customer> FindByIdAsync(string customerId)
        {
            Stripe.Customer stripeCustomer = await _stripe.CustomerService.GetAsync(customerId);
            return stripeCustomer;
        }

        public async Task<IEnumerable<Stripe.Customer>> GetAsync(string email)
        {
            return await _stripe.CustomerService.ListAsync(
                new Stripe.CustomerListOptions()
                {
                    Email = email
                }, 
                new Stripe.RequestOptions()
                {

                }
            );
        }

        public async Task SetDefaultCard(string customerId, string cardId)
        {
            var customer = new Stripe.CustomerUpdateOptions()
            {
                DefaultSource = cardId
            };
            Stripe.Customer stripeCustomer = await _stripe.CustomerService.UpdateAsync(customerId, customer);
        }

    }
}
