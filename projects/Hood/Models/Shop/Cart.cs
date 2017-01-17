using Hood.Models.Api;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Hood.Models
{
    public class Cart
    {
        /// <summary>
        /// The delivery charge.
        /// </summary>
        public decimal Delivery { get; set; }

        /// <summary>
        /// The number of items in the cart.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// The total amount of tax applied to the entire cart.
        /// </summary>
        public decimal Tax { get; set; }

        /// <summary>
        /// The total amount of discount applied to the entire cart.
        /// </summary>
        public decimal DiscountCart { get; set; }

        /// <summary>
        /// The total of the entire cart, including discount, but excluding tax.
        /// </summary>
        public decimal DiscountedTotalCart { get; set; }

        /// <summary>
        /// The subtotal of the entire cart, The total of the items, without tax and without discount.
        /// </summary>
        public decimal SubTotal { get; set; } 

        /// <summary>
        ///  The final total, including discount and tax and delivery charge.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        ///  The final total, including discount and less the tax.
        /// </summary>
        public decimal PreTaxTotal {
            get
            {
                return Total - Tax;
            }
        }

        /// <summary>
        /// The items in the cart.
        /// </summary>
        public List<CartItem> OrderLines { get; set; }

        public Cart()
        {
            Delivery = 0;
            Tax = 0;
            TotalItems = 0;
            SubTotal = 0;
            DiscountCart = 0;
            DiscountedTotalCart = 0;
            Total = 0;
            OrderLines = new List<CartItem>();
        }

        private void SetUpTotals()
        {
            Tax = 0;
            TotalItems = 0;
            SubTotal = 0;
            DiscountCart = 0;
            DiscountedTotalCart = 0;
            Total = 0;
            foreach (var item in OrderLines)
            {
                TotalItems += item.Quantity;
                SubTotal += item.LineSubTotal;
                Tax += item.LineTaxTotal;
                DiscountCart += item.LineDiscountTotal;
                DiscountedTotalCart += item.LineSubTotal - item.LineDiscountTotal;
                Total += item.LineTotal;
            }
            Total += Delivery; ;
        }

        internal void RemoveLine(Product product)
        {
            CartItem pLine = OrderLines.Where(c => c.ProductID == product.Id).FirstOrDefault();
            OrderLines.Remove(pLine);
            SetUpTotals();
        }

        internal void ClearCart()
        {
            OrderLines.Clear();
            SetUpTotals();
        }

        internal void UpdateLine(Product product, int qty)
        {
            CartItem pLine = OrderLines.Where(c => c.ProductID == product.Id).FirstOrDefault();

            if (pLine != null)
            {
                pLine.Quantity += qty;
            }
            else
            {
                if (product == null)
                    throw new Exception("There is no such product!");
                pLine = new CartItem()
                {
                    DiscountPercentage = product.Discount,
                    Discount = product.DiscountAmount,
                    DiscountedPrice = product.DiscountedPrice,
                    ProductID = product.Id,
                    Image = product.Api.FeaturedImage,
                    ItemBasePrice = product.BasePrice,
                    Price = product.Price,
                    Title = product.Title,
                    TaxPercentage = product.Tax,
                    Tax = product.TaxAmount,
                    Url = product.Api.Url,
                    Quantity = qty,
                };
                OrderLines.Add(pLine);
            }
            if (pLine.Quantity <= 0)
                OrderLines.Remove(pLine);
            else
            {
                pLine.LineSubTotal = pLine.Price * pLine.Quantity;
                pLine.LineDiscountTotal = pLine.Discount * pLine.Quantity;
                pLine.LineTaxTotal = pLine.Tax * pLine.Quantity;
                pLine.LineTotal = pLine.LineSubTotal - pLine.LineDiscountTotal;
            }
            SetUpTotals();
        }
    }
}
