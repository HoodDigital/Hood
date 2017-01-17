using System;
using PayPal;
using PayPal.Api;

namespace Hood.Models
{
    public class PayPalPaymentException : PayPalException
    {
        public Payment Payment { get; set; }
        public Transaction Transaction { get; set; }

        public PayPalPaymentException()
        {
        }

        public PayPalPaymentException(string message)
        : base(message)
        {
        }

        public PayPalPaymentException(string message, Exception inner)
        : base(message, inner)
        {
        }

    }

}
