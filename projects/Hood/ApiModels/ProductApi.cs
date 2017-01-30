using Hood.Extensions;
using Hood.Interfaces;
using System;

namespace Hood.Models.Api
{
    public class Product : Content, IMetaObect<ContentMeta>
    {
        public ContentApi Api { get; set; }

        public string SKU { get; set; }
        public string Supplier { get; set; }

        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount
        {
            get
            {
                var perc = Math.Round(Tax / 100, 2);
                return Math.Round(perc * DiscountedPrice, 2);
            }
        }
        public decimal BasePrice
        {
            get
            {
                return DiscountedPrice - TaxAmount;
            }
        }

        public decimal ListPrice { get; set; }

        public decimal Discount { get; set; }
        public decimal DiscountAmount
        {
            get
            {
                var perc = Math.Round(Discount / 100, 2);
                return Math.Round(perc * Price, 2);
            }
        }
        public decimal DiscountedPrice
        {
            get
            {
                return Price - DiscountAmount;
            }
        }


        public decimal RecommendedPrice { get; set; }

        public int MinCart { get; set; }
        public int MaxCart { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
        public int Weight { get; set; }
        public int StockLevel { get; set; }

        public bool Available { get; set; }
        public bool TrackStock { get; set; }
        public bool SellByDate { get; set; }

        public DateTime SellStartDate { get; set; }
        public DateTime SellEndDate { get; set; }

        public Product(Content contentBase)
        {
            if (contentBase == null)
                return;
            contentBase.CopyProperties(this);
            //Api = new ContentApi(contentBase);
            this.Metadata = contentBase.Metadata;
        }
    }
}
