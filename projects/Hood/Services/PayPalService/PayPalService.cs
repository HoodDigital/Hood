using Microsoft.Extensions.Configuration;
using PayPal.Api;
using System.Collections.Generic;
using Hood.Models;

namespace Hood.Services
{
    /// <summary>
    /// Configuration and access tokens for accessing the PayPal Api
    /// </summary>
    public class PayPalService : IPayPalService
    {
        private readonly IConfiguration _config;

        public PayPalService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Helper method for getting a currency amount.
        /// </summary>
        /// <param name="value">The value for the currency object.</param>
        /// <returns></returns>
        public Currency GetCurrency(string value)
        {
            return new Currency() { value = value, currency = _config["PayPal:Currency"] };
        }

        /// <summary>
        /// Create the configuration map that contains mode and other optional configuration details.
        /// </summary>
        /// <returns>Dictionary of configuration items</returns>
        public Dictionary<string, string> GetConfig()
        {
            var sdkConfig = new Dictionary<string, string> {
               { "mode", _config["PayPal:Mode"] },
               { "clientId", _config["PayPal:ClientId"] },
               { "clientSecret", _config["PayPal:ClientSecret"] }
            };
            return sdkConfig;
        }

        /// <summary>
        /// Create accessToken
        /// </summary>
        /// <returns>Access Token as a string</returns>
        private string GetAccessToken()
        {
            var accessToken = new OAuthTokenCredential(_config["PayPal:ClientId"], _config["PayPal:ClientSecret"], GetConfig()).GetAccessToken();
            return accessToken;
        }

        /// <summary>
        /// Returns APIContext object
        /// </summary>
        /// <returns>The PayPal Api Context</returns>
        public APIContext GetAPIContext()
        {
            APIContext apiContext = new APIContext(GetAccessToken());
            apiContext.Config = GetConfig();
            return apiContext;
        }

        /// <summary>
        /// Creates the payment for paypal transactions. 
        /// </summary>
        /// <param name="apiContext"></param>
        /// <param name="redirectUrl"></param>
        /// <returns></returns>
        public Payment CreatePayPalPayment(APIContext apiContext, RedirectUrls redirects, Transaction transaction)
        {
            var payer = new Payer() { payment_method = "paypal" };

            var transactionList = new List<Transaction>();

            transactionList.Add(transaction);

            Payment payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirects
            };

            // Create a payment using a APIContext
            return payment.Create(apiContext);
        }

        /// <summary>
        /// Executes the payment for paypal transactions, once the payment is created and the return Url has been hit.
        /// </summary>
        /// <param name="apiContext"></param>
        /// <param name="payerId"></param>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public Payment ExecutePayPalPayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            Payment payment = new Payment() { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }

        /// <summary>
        /// Converts the Hood Shopping Cart into a transaction with a list of items, totals and delivery set up for use with PayPal API
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        public Transaction CreateTransactionFromCart(Cart cart, string orderId)
        {

            //similar to credit card create itemlist and add item objects to it
            var itemList = new ItemList() { items = new List<Item>() };
            foreach (CartItem itm in cart.OrderLines)
            {
                itemList.items.Add(new Item()
                {
                    name = itm.Title.ToString(),
                    currency = _config["PayPal:Currency"],
                    price = itm.ItemBasePrice.ToString("N2"),
                    quantity = itm.Quantity.ToString(),
                    tax = itm.Tax.ToString("N2"),
                    sku = itm.ProductID.ToString()
                });
            }

            // similar as we did for credit card, do here and create details object
            var details = new Details()
            {
                tax = cart.Tax.ToString("N2"),
                shipping = cart.Delivery.ToString("N2"),
                subtotal = cart.PreTaxTotal.ToString("N2")
            };

            // similar as we did for credit card, do here and create amount object
            var amount = new Amount()
            {
                details = details,
                currency = _config["PayPal:Currency"],
                total = cart.Total.ToString("N2") // Total must be equal to sum of shipping, tax and subtotal.
            };


            var transaction = new Transaction()
            {
                description = "Order #" + orderId.ToString(),
                invoice_number = orderId.ToString(),
                amount = amount,
                item_list = itemList
            };

            return transaction;
        }

        /// <summary>
        /// Create a billing plan. 
        /// </summary>
        /// <param name="redirects"></param>
        /// <returns></returns>
        public Plan CreatePlanObject(RedirectUrls redirects)
        {
            // ### Create the Billing Plan
            // Both the trial and standard plans will use the same shipping
            // charge for this example, so for simplicity we'll create a
            // single object to use with both payment definitions.
            var shippingChargeModel = new ChargeModel()
            {
                type = "SHIPPING",
                amount = GetCurrency("9.99")
            };

            // Define the plan and attach the payment definitions and merchant preferences.
            // More Information: https://developer.paypal.com/webapps/developer/docs/api/#create-a-plan
            return new Plan
            {
                name = "T-Shirt of the Month Club Plan",
                description = "Monthly plan for getting the t-shirt of the month.",
                type = "fixed",
                // Define the merchant preferences.
                // More Information: https://developer.paypal.com/webapps/developer/docs/api/#merchantpreferences-object
                merchant_preferences = new MerchantPreferences()
                {
                    setup_fee = GetCurrency("1"),
                    return_url = redirects.return_url,
                    cancel_url = redirects.cancel_url,
                    auto_bill_amount = "YES",
                    initial_fail_amount_action = "CONTINUE",
                    max_fail_attempts = "0"
                },
                payment_definitions = new List<PaymentDefinition>
                {
                    // Define a trial plan that will only charge $9.99 for the first
                    // month. After that, the standard plan will take over for the
                    // remaining 11 months of the year.
                    new PaymentDefinition()
                    {
                        name = "Trial Plan",
                        type = "TRIAL",
                        frequency = "MONTH",
                        frequency_interval = "1",
                        amount = GetCurrency("9.99"),
                        cycles = "1",
                        charge_models = new List<ChargeModel>
                        {
                            new ChargeModel()
                            {
                                type = "TAX",
                                amount = GetCurrency("1.65")
                            },
                            shippingChargeModel
                        }
                    },

                    // Define the standard payment plan. It will represent a monthly
                    // plan for $19.99 USD that charges once month for 11 months.
                    new PaymentDefinition
                    {
                        name = "Standard Plan",
                        type = "REGULAR",
                        frequency = "MONTH",
                        frequency_interval = "1",
                        amount = GetCurrency("19.99"),
                        // > NOTE: For `IFNINITE` type plans, `cycles` should be 0 for a `REGULAR` `PaymentDefinition` object.
                        cycles = "11",
                        charge_models = new List<ChargeModel>
                        {
                            new ChargeModel
                            {
                                type = "TAX",
                                amount = GetCurrency("2.47")
                            },
                            shippingChargeModel
                        }
                    }
                }
            };
        }
    }
}