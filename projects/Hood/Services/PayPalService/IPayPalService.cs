using Hood.Models;
using PayPal.Api;
using System.Collections.Generic;

namespace Hood.Services
{
    public interface IPayPalService
    {
        Dictionary<string, string> GetConfig();
        APIContext GetAPIContext();

        Payment CreatePayPalPayment(APIContext apiContext, RedirectUrls redirects, Transaction transaction);
        Payment ExecutePayPalPayment(APIContext apiContext, string payerId, string paymentId);

        Transaction CreateTransactionFromCart(Cart cart, string orderId);
    }
}