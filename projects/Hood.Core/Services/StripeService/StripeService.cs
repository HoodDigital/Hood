using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public SetupIntentService SetupIntentService => new SetupIntentService(Client);
        public PaymentIntentService PaymentIntentService => new PaymentIntentService(Client);
        public PaymentMethodService PaymentMethodService => new PaymentMethodService(Client);
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
                {
                    return null;
                }

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
                {
                    return null;
                }

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
                {
                    return null;
                }

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
                {
                    return null;
                }

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
                {
                    return null;
                }

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
                {
                    return null;
                }

                throw stripeEx;
            }
        }
        public async Task<Plan> MovePlanToProductAsync(string planId, string productId)
        {
            PlanUpdateOptions updateOptions = new PlanUpdateOptions()
            {
                Product = productId
            };
            return await PlanService.UpdateAsync(planId, updateOptions);
        }
        #endregion

        #region Customers
        public async Task<Customer> CreateCustomerAsync(ApplicationUser user)
        {
            CustomerCreateOptions customer = new CustomerCreateOptions()
            {
                Email = user.Email,
                Phone = user.PhoneNumber,
                Address = user.BillingAddress.ToStripeAddress(),
                Name = user.ToAdminName()
            };
            return await CustomerService.CreateAsync(customer);
        }
        public async Task<Customer> UpdateCustomerAsync(string customerId, ApplicationUser user)
        {
            CustomerUpdateOptions customer = new CustomerUpdateOptions()
            {
                Email = user.Email,
                Phone = user.PhoneNumber,
                Address = user.BillingAddress.ToStripeAddress(),
                Name = user.ToAdminName()
            };
            return await CustomerService.UpdateAsync(customerId, customer);
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
                {
                    return null;
                }

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
                {
                    return null;
                }

                throw stripeEx;
            }
        }
        public async Task<Customer> GetCustomerByEmailAsync(string email)
        {
            StripeList<Customer> customers = await CustomerService.ListAsync(new CustomerListOptions() { Email = email });
            if (customers != null && customers.Data != null && customers.Data.Count > 0)
            {
                var customer = customers.First();
                if (customer.Email == email)
                    return customer;
            }
            return null;
        }
        public async Task<List<Customer>> GetCustomersByEmailAsync(string email)
        {
            StripeList<Customer> customers = await CustomerService.ListAsync(new CustomerListOptions() { Email = email });
            return customers != null ? customers.Data : new List<Customer>();
        }
        #endregion

        #region Payment Methods
        public async Task<Customer> SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId)
        {
            var source = await PaymentMethodService.GetAsync(paymentMethodId);

            CustomerUpdateOptions updateOptions = new CustomerUpdateOptions()
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions()
                {
                    DefaultPaymentMethod = source.Id
                }
            };
            return await CustomerService.UpdateAsync(customerId, updateOptions);
        }
        public async Task<PaymentMethod> DeletePaymentMethodAsync(string customerId, string paymentMethodId)
        {
            try
            {
                return await PaymentMethodService.DetachAsync(paymentMethodId, new PaymentMethodDetachOptions());
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw stripeEx;
            }
        }
        public async Task<PaymentMethod> GetPaymentMethodByIdAsync(string paymentMethodId)
        {
            try
            {
                return await PaymentMethodService.GetAsync(paymentMethodId);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw stripeEx;
            }
        }
        public async Task<List<PaymentMethod>> GetAllPaymentMethodsAsync(string customerId, string type)
        {
            StripeList<PaymentMethod> pms = await PaymentMethodService.ListAsync(new PaymentMethodListOptions()
            {
                Customer = customerId,
                Type = type,
                Expand = new List<string>() { "data.customer" }
            });
            return pms.Data;
        }
        #endregion

        #region Invoices 
        public async Task<Invoice> GetInvoiceByIdAsync(string invoiceId)
        {
            try
            {
                var options = new InvoiceGetOptions();
                options.AddExpand("data.payment_intent");
                return await InvoiceService.GetAsync(invoiceId);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw stripeEx;
            }
        }
        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync(string customerId, string startAfterId, int? pageSize = null)
        {
            InvoiceListOptions options = new InvoiceListOptions()
            {
                Customer = customerId,
                StartingAfter = startAfterId,
                Limit = pageSize
            };
            options.AddExpand("data.payment_intent");
            return await InvoiceService.ListAsync(options);
        }
        public async Task<Invoice> GetUpcomingInvoiceAsync(string customerId)
        {
            UpcomingInvoiceOptions options = new UpcomingInvoiceOptions()
            {
                Customer = customerId
            };
            options.AddExpand("data.payment_intent");
            return await InvoiceService.UpcomingAsync(options);
        }
        #endregion

        #region Subscriptions 
        public async Task<IEnumerable<Stripe.Subscription>> GetSusbcriptionsByCustomerIdAsync(string customerId, string planId = null)
        {
            SubscriptionListOptions options = new Stripe.SubscriptionListOptions()
            {
                Customer = customerId,
                Plan = planId
            };
            return await SubscriptionService.ListAsync(options);
        }
        public async Task<Stripe.Subscription> GetSusbcriptionByIdAsync(string subscriptionId)
        {
            try
            {
                SubscriptionGetOptions options = new SubscriptionGetOptions();
                options.AddExpand("latest_invoice.payment_intent");
                return await SubscriptionService.GetAsync(subscriptionId, options);
            }
            catch (StripeException stripeEx)
            {
                if (stripeEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw stripeEx;
            }
        }
        public async Task<Stripe.Subscription> AddCustomerToPlan(string customerId, string planId, int quantity = 1, string paymentMethodId = null)
        {
            var plan = await PlanService.GetAsync(planId);
            if (plan == null)
                throw new Exception("The plan could not be found on Stripe.");

            List<SubscriptionItemOptions> items = new List<SubscriptionItemOptions> {
                    new SubscriptionItemOptions {
                        Plan = plan.Id,
                        Quantity = quantity
                    }
                };

            SubscriptionCreateOptions options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = items,
                TrialFromPlan = true
            };            

            if (paymentMethodId != null)
            {
                options.DefaultPaymentMethod = paymentMethodId;
            }

            options.AddExpand("latest_invoice.payment_intent");
            return await SubscriptionService.CreateAsync(options);
        }
        public async Task<Stripe.Subscription> CancelSubscriptionAsync(string subscriptionId, bool cancelAtPeriodEnd = true, bool invoiceNow = false, bool prorate = false)
        {
            if (!cancelAtPeriodEnd)
            {
                return await SubscriptionService.CancelAsync(subscriptionId, new SubscriptionCancelOptions()
                {
                    InvoiceNow = invoiceNow,
                    Prorate = prorate
                });
            }

            return await SubscriptionService.UpdateAsync(subscriptionId, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true,
            });
        }
        public async Task<Stripe.Subscription> ReactivateSubscriptionAsync(string subscriptionId)
        {
            Stripe.Subscription subscription = await GetSusbcriptionByIdAsync(subscriptionId);
            if (subscription == null)
            {
                throw new Exception("Could not load Stripe subscription object to update.");
            }

            List<SubscriptionItemOptions> items = new List<SubscriptionItemOptions>();
            subscription.Items.ForEach(i => items.Add(new SubscriptionItemOptions
            {
                Id = i.Id,
                Plan = i.Plan.Id
            }));
            SubscriptionUpdateOptions options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false,
                Items = items,
            };
            return await SubscriptionService.UpdateAsync(subscriptionId, options);
        }
        public async Task<Stripe.Subscription> SwitchSubscriptionPlanAsync(string subscriptionId, string newPlanId)
        {
            Stripe.Subscription subscription = await GetSusbcriptionByIdAsync(subscriptionId);
            if (subscription == null)
            {
                throw new Exception("Could not load Stripe subscription object to update.");
            }

            Plan newPlan = await GetPlanByIdAsync(newPlanId);
            if (newPlan == null)
            {
                throw new Exception("There was a problem loading the subscription plan.");
            }

            SubscriptionItem oldPlanItem = subscription.Items.Data.FirstOrDefault(s => s.Plan.ProductId == newPlan.ProductId);
            if (oldPlanItem == null)
            {
                throw new Exception("Could not find a plan from the same product to switch from in the given subscription");
            }

            List<SubscriptionItemOptions> items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions()
                {
                    Id = oldPlanItem.Id,
                    Plan = newPlan.Id                    
                }
            };
            SubscriptionUpdateOptions options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false,
                Items = items,
            };

            options.TrialEnd = SubscriptionTrialEnd.Now;

            if (subscription.Status == SubscriptionStatuses.Trialing)
            {
                // how many days of trial are used?
                int daysUsed = (int)(DateTime.UtcNow - subscription.TrialStart.Value).TotalDays;
                if (newPlan.TrialPeriodDays > 0)
                {
                    int newTrialLength = (int)(newPlan.TrialPeriodDays - daysUsed);
                    if (newTrialLength > 0)
                    {
                        options.TrialEnd = DateTime.Now.AddDays(newTrialLength);
                    }
                }
            }

            return await SubscriptionService.UpdateAsync(subscriptionId, options);
        }
        #endregion
    }
}