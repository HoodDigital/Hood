using Hood.Models.Api;

namespace Hood.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }

        public MediaApi Image { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }

        /// <summary>
        /// The display price of the item. Including tax.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The price of the item. Excluding tax.
        /// </summary>
        public decimal ItemBasePrice { get; set; }

        /// <summary>
        /// The tax as a percentage of this item's price.
        /// </summary>
        public decimal TaxPercentage { get; set; }

        /// <summary>
        /// The discount as a percentage to be applied to this item.
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// The quantity of the items in this line.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The tax included in this item.
        /// </summary>
        public decimal Tax { get; set; }

        /// <summary>
        /// The discount applied to this item (per item).
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// The discounted price of this item.
        /// </summary>
        public decimal DiscountedPrice { get; set; }

        /// <summary>
        /// The subtotal of the items in the line, without tax and without discount.
        /// </summary>
        public decimal LineSubTotal { get; set; }

        /// <summary>
        /// The total of the tax of the items in the line.
        /// </summary>
        public decimal LineTaxTotal { get; set; }

        /// <summary>
        /// The total of the discount of the items in the line
        /// </summary>
        public decimal LineDiscountTotal { get; set; }

        /// <summary>
        /// The total, including discount and tax, of the items in the line.
        /// </summary>
        public decimal LineTotal { get; set; }
    }
}
