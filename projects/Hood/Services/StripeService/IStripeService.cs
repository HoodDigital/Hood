using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hood.Models;
using Stripe;

namespace Hood.Services
{
    public interface IStripeService
    {
        #region Services 
        ProductService ProductService { get; }
        PlanService PlanService { get; }
        CustomerService CustomerService { get; }
        CardService CardService { get; }
        InvoiceService InvoiceService { get; }
        SubscriptionService SubscriptionService { get; }
        RefundService RefundService { get; }
        TokenService TokenService { get; }
        #endregion

        #region Products
        Task<IEnumerable<Product>> GetAllProductsAsync(ProductListOptions options = null);

        Task<Product> GetProductByIdAsync(string subscriptionId, ProductGetOptions options = null);

        Task<Product> CreateProductAsync(string name, string type = "service", bool active = true);

        Task<Product> UpdateProductAsync(string productId, ProductUpdateOptions options);

        Task<Product> DeleteProductAsync(string productId);
        #endregion

        #region Plans
        Task<IEnumerable<Plan>> GetAllPlansAsync(PlanListOptions options = null);

        Task<Plan> GetPlanByIdAsync(string subscriptionId, PlanGetOptions options = null);

        Task<Plan> CreatePlanAsync(string name, int amount, string currency = "gbp", string interval = "month", int intervalCount = 1, int? trialPeriodDays = 30, string productId = null, bool active = true);

        Task<Plan> UpdatePlanAsync(string planId, PlanUpdateOptions options);

        Task<Plan> DeletePlanAsync(string planId);

        Task<Plan> MovePlanToProductAsync(string planId, string productId);
        #endregion

        #region Customers
        Task<Customer> CreateCustomerAsync(ApplicationUser user, string token, string planId = null);

        Task<Customer> UpdateCustomerAsync(string customerId, CustomerUpdateOptions options);

        Task<Customer> DeleteCustomerAsync(string customerId);

        Task<Customer> GetCustomerByIdAsync(string customerId);

        Task<List<Customer>> GetCustomersByEmailAsync(string email);

        Task<Customer> SetDefaultCardAsync(string customerId, string cardId);
        #endregion

        #region Payment Methods
        Task<Card> CreateCardAsync(string customerId, string token);

        Task<Card> DeleteCardAsync(string customerId, string cardId);

        Task<Stripe.Card> GetCardByIdAsync(string customerId, string cardId);

        Task<IEnumerable<Stripe.Card>> GetAllCardsAsync(string customerId);
        #endregion

        #region Invoices 
        Task<Invoice> GetInvoiceByIdAsync(string invoiceId);

        Task<IEnumerable<Invoice>> GetAllInvoicesAsync(string customerId, string startAfterId, int? pageSize = null);

        Task<Invoice> GetUpcomingInvoiceAsync(string customerId);
        #endregion

        #region Subscriptions 
        Task<IEnumerable<Stripe.Subscription>> GetSusbcriptionsByCustomerIdAsync(string customerId);
        Task<Stripe.Subscription> GetSusbcriptionByIdAsync(string subscriptionId);
        Task<Stripe.Subscription> AddCustomerToPlan(string customerId, string planId, int quantity = 1, DateTime? trialEnd = null);
        Task<Stripe.Subscription> CancelSubscriptionAsync(string subscriptionId, bool cancelAtPeriodEnd = true, bool invoiceNow = false, bool prorate = false);
        Task<Stripe.Subscription> ReactivateSubscriptionAsync(string subscriptionId);
        Task<Stripe.Subscription> SwitchSubscriptionPlanAsync(string subscriptionId, string newPlanId);
        #endregion
    }
}