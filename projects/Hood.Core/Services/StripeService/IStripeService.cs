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
        SetupIntentService SetupIntentService { get; }
        PaymentIntentService PaymentIntentService { get; }
        PaymentMethodService PaymentMethodService { get; }
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
        Task<Customer> CreateCustomerAsync(ApplicationUser user);
        Task<Customer> UpdateCustomerAsync(string customerId, ApplicationUser user);
        Task<Customer> DeleteCustomerAsync(string customerId);
        Task<Customer> GetCustomerByIdAsync(string customerId);
        Task<Customer> GetCustomerByEmailAsync(string email);
        Task<List<Customer>> GetCustomersByEmailAsync(string email);
        #endregion

        #region Payment Methods
        Task<Customer> SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId);
        Task<PaymentMethod> DeletePaymentMethodAsync(string customerId, string paymentMethodId);
        Task<PaymentMethod> GetPaymentMethodByIdAsync(string paymentMethodId);
        Task<List<PaymentMethod>> GetAllPaymentMethodsAsync(string customerId, string type);
        #endregion

        #region Invoices 
        Task<Invoice> GetInvoiceByIdAsync(string invoiceId);

        Task<IEnumerable<Invoice>> GetAllInvoicesAsync(string customerId, string startAfterId, int? pageSize = null);

        Task<Invoice> GetUpcomingInvoiceAsync(string customerId);
        #endregion

        #region Subscriptions 
        Task<IEnumerable<Stripe.Subscription>> GetSusbcriptionsByCustomerIdAsync(string customerId, string planId = null);
        Task<Stripe.Subscription> GetSusbcriptionByIdAsync(string subscriptionId);
        Task<Stripe.Subscription> AddCustomerToPlan(string customerId, string planId, int quantity = 1, string paymentMethodId = null);
        Task<Stripe.Subscription> CancelSubscriptionAsync(string subscriptionId, bool cancelAtPeriodEnd = true, bool invoiceNow = false, bool prorate = false);
        Task<Stripe.Subscription> ReactivateSubscriptionAsync(string subscriptionId);
        Task<Stripe.Subscription> SwitchSubscriptionPlanAsync(string subscriptionId, string newPlanId);
        #endregion
    }
}