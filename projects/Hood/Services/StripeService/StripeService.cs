using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class StripeService : IStripeService
    {
        private string StripeApiKey { get; set; }
        public StripeService()
        {
            BillingSettings settings = Engine.Settings.Billing;
            if (settings.EnableStripeTestMode)
            {
                StripeApiKey = settings.StripeTestKey;
            }
            else
            {
                StripeApiKey = settings.StripeLiveKey;
            }
        }

        protected StripeClient Client => new StripeClient(StripeApiKey);

        #region Services 
        public ProductService ProductService => new ProductService(Client);
        public PlanService PlanService => new PlanService(Client);
        public CustomerService CustomerService => new CustomerService(Client);
        public CardService CardService => new CardService(Client);
        public InvoiceService InvoiceService => new InvoiceService(Client);
        public SubscriptionService SubscriptionService => new SubscriptionService(Client);
        public RefundService RefundService => new RefundService(Client);
        public TokenService TokenService => new TokenService(Client);
        #endregion

        #region Products
        public async Task<IEnumerable<Product>> GetAllProductsAsync(ProductListOptions options = null)
        {
            return await ProductService.ListAsync(options);
        }

        public async Task<Product> GetProductByIdAsync(string subscriptionId, ProductGetOptions options = null)
        {
            try
            {
                return await ProductService.GetAsync(subscriptionId, options);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw stripeEx;
            }
        }

        public async Task<Product> CreateProductAsync(string name, string type = "service", bool active = true)
        {
            ProductCreateOptions product = new ProductCreateOptions()
            {
                Active = active,
                Name = name, 
                Type = type
            };
            return await ProductService.CreateAsync(product);
        }

        public async Task<Product> UpdateProductAsync(string productId, ProductUpdateOptions options)
        {
            try
            {
                return await ProductService.UpdateAsync(productId, options);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw stripeEx;
            }
        }

        public async Task<Product> DeleteProductAsync(string productId)
        {
            try
            {
                return await ProductService.DeleteAsync(productId);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw stripeEx;
            }
        }
        #endregion

        #region Plans
        public async Task<IEnumerable<Plan>> GetAllPlansAsync(PlanListOptions options = null)
        {
            return await PlanService.ListAsync(options);
        }

        public async Task<Plan> GetPlanByIdAsync(string subscriptionId, PlanGetOptions options = null)
        {
            try
            {
                return await PlanService.GetAsync(subscriptionId, options);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw stripeEx;
            }
        }

        public async Task<Plan> CreatePlanAsync(string name, int amount, string currency = "gbp", string interval = "month", int intervalCount = 1, int? trialPeriodDays = 30, string productId = null, bool active = true)
        {
            AnyOf<string, PlanProductCreateOptions> product = productId.IsSet() ?
                new AnyOf<string, PlanProductCreateOptions>(productId) :
                new AnyOf<string, PlanProductCreateOptions>(new PlanProductCreateOptions() { Name = name });

            PlanCreateOptions myPlan = new PlanCreateOptions()
            {
                Amount = amount,                     // all amounts on Stripe are in cents, pence, etc
                Currency = currency,                 // "usd" only supported right now
                Interval = interval,                 // "month" or "year"
                IntervalCount = intervalCount,       // optional
                Nickname = name,
                TrialPeriodDays = trialPeriodDays,   // amount of time that will lapse before the customer is billed
                Product = product,
                Active = active
            };

            return await PlanService.CreateAsync(myPlan);
        }

        public async Task<Plan> UpdatePlanAsync(string planId, PlanUpdateOptions options)
        {
            try
            {
                return await PlanService.UpdateAsync(planId, options);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw stripeEx;
            }
        }

        public async Task<Plan> DeletePlanAsync(string planId)
        {
            try
            {
                return await PlanService.DeleteAsync(planId);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw stripeEx;
            }
        }

        public async Task<Plan> MovePlanToProductAsync(string planId, string productId)
        {
            PlanUpdateOptions updateOptions = new PlanUpdateOptions()
            {
                ProductId = productId
            };
            return await PlanService.UpdateAsync(planId, updateOptions);
        }
        #endregion

        #region Customers
        public async Task<Customer> CreateCustomerAsync(ApplicationUser user, string token, string planId = null)
        {
            CustomerCreateOptions customer = new CustomerCreateOptions()
            {
                Email = user.Email,
                Description = $"{user.ToAdminName()} - {user.UserName} ({user.Email})",
                Name = user.ToAdminName(),
                Source = token,
                PlanId = planId
            };
            return await CustomerService.CreateAsync(customer);
        }

        public async Task<Customer> UpdateCustomerAsync(string customerId, CustomerUpdateOptions options)
        {
            return await CustomerService.UpdateAsync(customerId, options);
        }

        public async Task<Customer> DeleteCustomerAsync(string customerId)
        {
            try
            {
                return await CustomerService.DeleteAsync(customerId);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw stripeEx;
            }
        }

        public async Task<Customer> GetCustomerByIdAsync(string customerId)
        {
            try
            {
                return await CustomerService.GetAsync(customerId);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw stripeEx;
            }
        }

        public async Task<List<Customer>> GetCustomersByEmailAsync(string email)
        {
            var customers = await CustomerService.ListAsync(new CustomerListOptions() { Email = email });
            return customers != null ? customers.Data : new List<Customer>();
        }

        public async Task<Customer> SetDefaultCardAsync(string customerId, string cardId)
        {
            CustomerUpdateOptions updateOptions = new CustomerUpdateOptions()
            {
                DefaultSource = cardId
            };
            return await CustomerService.UpdateAsync(customerId, updateOptions);
        }
        #endregion

        #region Payment Methods
        public async Task<Card> CreateCardAsync(string customerId, string token)
        {
            CardCreateOptions card = new Stripe.CardCreateOptions()
            {
                Source = token
            };
            return await CardService.CreateAsync(customerId, card);
        }

        public async Task<Card> DeleteCardAsync(string customerId, string cardId)
        {
            return await CardService.DeleteAsync(customerId, cardId);
        }

        public async Task<Stripe.Card> GetCardByIdAsync(string customerId, string cardId)
        {
            try
            {
                return await CardService.GetAsync(customerId, cardId);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw stripeEx;
            }
        }

        public async Task<IEnumerable<Stripe.Card>> GetAllCardsAsync(string customerId)
        {
            return await CardService.ListAsync(customerId);
        }
        #endregion

        #region Invoices 
        public async Task<Invoice> GetInvoiceByIdAsync(string invoiceId)
        {
            try
            {
                return await InvoiceService.GetAsync(invoiceId);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw stripeEx;
            }
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync(string customerId, string startAfterId, int? pageSize = null)
        {
            InvoiceListOptions options = new InvoiceListOptions()
            {
                CustomerId = customerId,
                StartingAfter = startAfterId,
                Limit = pageSize
            };
            return await InvoiceService.ListAsync(options);
        }

        public async Task<Invoice> GetUpcomingInvoicesAsync(string customerId)
        {
            UpcomingInvoiceOptions options = new UpcomingInvoiceOptions()
            {
                CustomerId = customerId
            };
            InvoiceService invoiceService = new InvoiceService(Client);
            return await invoiceService.UpcomingAsync(options);
        }
        #endregion

        #region Subscriptions 
        public async Task<IEnumerable<Stripe.Subscription>> GetSusbcriptionsByCustomerIdAsync(string customerId)
        {
            SubscriptionListOptions options = new Stripe.SubscriptionListOptions()
            {
                CustomerId = customerId
            };
            return await SubscriptionService.ListAsync(options);
        }

        public async Task<Stripe.Subscription> GetSusbcriptionByIdAsync(string subscriptionId)
        {
            try
            {
                return await SubscriptionService.GetAsync(subscriptionId);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                throw stripeEx;
            }
        }

        public async Task<Stripe.Subscription> SubscribeUserAsync(string customerId, string planId, DateTime? trialEnd = null)
        {
            SubscriptionCreateOptions options = new SubscriptionCreateOptions()
            {
                TrialEnd = trialEnd,
                CustomerId = customerId,
                Items = new List<SubscriptionItemOption>()
                {
                    new SubscriptionItemOption()
                    {
                        PlanId = planId,
                        Quantity = 1
                    }
                }
            };
            return await SubscriptionService.CreateAsync(options);
        }

        public async Task<Stripe.Subscription> UpdateSubscriptionAsync(string subscriptionId, Plan plan)
        {
            Stripe.Subscription currentSubscription = await SubscriptionService.GetAsync(subscriptionId);
            SubscriptionUpdateOptions updateOptions = new SubscriptionUpdateOptions()
            {
                Items = new List<SubscriptionItemUpdateOption>()
                {
                    new SubscriptionItemUpdateOption()
                    {
                        Id = subscriptionId
                    }
                }
            };
            if (currentSubscription.Status == "trialing" && plan.TrialPeriodDays > 0)
            {
                // if the current trialEnd is before the new subscription's trial WOULD end
                DateTime newTrialEnd = DateTime.Now.AddDays(plan.TrialPeriodDays.Value);
                if (newTrialEnd < currentSubscription.TrialEnd)
                {
                    updateOptions.TrialEnd = newTrialEnd;
                }
                else
                {
                    updateOptions.TrialEnd = currentSubscription.TrialEnd;
                }
            }
            else
            {
                updateOptions.TrialEnd = DateTime.Now;
            }
            currentSubscription = await SubscriptionService.UpdateAsync(subscriptionId, updateOptions);
            return currentSubscription;
        }

        public async Task<Stripe.Subscription> CancelSubscriptionAsync(string subscriptionId, bool cancelAtPeriodEnd = true)
        {
            if (!cancelAtPeriodEnd)
            {
                return await SubscriptionService.CancelAsync(subscriptionId, new SubscriptionCancelOptions() { });
            }

            await SubscriptionService.UpdateAsync(subscriptionId, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false,
            });
            return await SubscriptionService.CancelAsync(subscriptionId, new SubscriptionCancelOptions() { });

        }
        #endregion
    }
}