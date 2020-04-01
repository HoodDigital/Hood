using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models.Payments
{
    public partial class Order : Order<Address>
    {}

    public partial class Order<TAddress> : BaseEntity<string>
        where TAddress : IAddress, new()
    {
        /// <summary>
        /// Unique reference for this transaction.
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// The type of the transaction (e.g. Payment, Deferred, Repeat or Refund.)
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// A brief description of the goods or services purchased.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Result of transaction. 
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Code related to the status of the transaction.
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// A detailed reason for the status of the transaction.
        /// </summary>
        public string StatusDetail { get; set; }

        /// <summary>
        ///  The currency of the amount in 3 letter ISO 4217 format. e.g. GBP for Pound Sterling.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Provides information regarding the transaction amount. This is only returned in GET requests.
        /// </summary>
        [NotMapped]
        public OrderAmount Amount
        {
            get { return AmountJson.IsSet() ? JsonConvert.DeserializeObject<OrderAmount>(AmountJson) : null; }
            set { AmountJson = JsonConvert.SerializeObject(value); }
        }
        public string AmountJson { get; set; }

        /// <summary>
        /// Payment method for this transaction.
        /// </summary>
        [NotMapped]
        public PaymentMethod PaymentMethod
        {
            get { return PaymentMethodJson.IsSet() ? JsonConvert.DeserializeObject<PaymentMethod>(PaymentMethodJson) : null; }
            set { PaymentMethodJson = JsonConvert.SerializeObject(value); }
        }
        public string PaymentMethodJson { get; set; }

        /// <summary>
        /// Customer’s first names.
        /// </summary>
        [JsonProperty(PropertyName = "customerFirstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// Customer’s last name.
        /// </summary>
        [JsonProperty(PropertyName = "customerLastName")]
        public string LastName { get; set; }

        /// <summary>
        /// Customer’s email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Customer’s phone number.
        /// </summary>
        public string Phone { get; set; }


        public DateTime CreatedOn { get; set; }

        [NotMapped]
        public List<OrderNote> Notes
        {
            get { return NotesJson.IsSet() ? JsonConvert.DeserializeObject<List<OrderNote>>(NotesJson) : new List<OrderNote>(); }
            set { NotesJson = JsonConvert.SerializeObject(value); }
        }
        public string NotesJson { get; set; }

        [NotMapped]
        public TAddress BillingAddress
        {
            get { return BillingAddressJson.IsSet() ? JsonConvert.DeserializeObject<TAddress>(BillingAddressJson) : default(TAddress); }
            set { BillingAddressJson = JsonConvert.SerializeObject(value); }
        }
        public string BillingAddressJson { get; set; }

        [JsonIgnore]
        public bool UseBilling { get; set; }

        [NotMapped]
        public TAddress ShippingAddress
        {
            get { return !UseBilling && ShippingAddressJson.IsSet() ? JsonConvert.DeserializeObject<TAddress>(ShippingAddressJson) : default(TAddress); }
            set { ShippingAddressJson = JsonConvert.SerializeObject(value); }
        }
        public string ShippingAddressJson { get; set; }
    }
}
