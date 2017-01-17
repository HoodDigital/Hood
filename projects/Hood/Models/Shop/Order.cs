using System;
using System.Collections.Generic;

namespace Hood.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public string UserID { get; set; }
        public int? BillingAddressID { get; set; }
        public int AddressID { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string Callbacks { get; set; }
        public DateTime DateAdded { get; set; }

        public string TransactionID { get; set; }

        public string SpecialRequirements { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal DeliveryCharge { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }

        public virtual Address DeliveryAddress { get; set; }
        public virtual Address BillingAddress { get; set; }
        public virtual ApplicationUser AspNetUser { get; set; }
        public virtual List<CartItem> OrderLines { get; set; }

        public Order()
        {
            
        }

    }
}
