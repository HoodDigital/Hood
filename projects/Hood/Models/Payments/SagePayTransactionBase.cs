using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models.Payments
{
    public partial class SagePayTransactionBase : BaseOrder<SagePayAddress>
    {
        /// <summary>
        /// Your SagePay vendor account name.
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Sage Pay’s unique reference for this transaction.
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// The type of the transaction (e.g. Payment, Deferred, Repeat or Refund.)
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Your unique reference for this transaction. Maximum of 40 characters.
        /// </summary>
        public string VendorTxCode => Id;

        /// <summary>
        /// A brief description of the goods or services purchased.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///  Identifies the customer has ticked a box to indicate that they wish to receive tax back on their donation. 
        /// </summary>
        public string GiftAid { get; set; }

        /// <summary>
        /// The method used to capture card data. [Ecommerce] 
        /// </summary>
        [NotMapped]
        public string EntryMethod => "Ecommerce";

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

        /// <summary>
        /// This can be used to send the unique reference for the partner that referred the merchant to Sage Pay. Maximum of 40 characters.
        /// </summary>
        public string ReferrerId { get; set; }

        /// <summary>
        /// Merchant Session object, for passing the merchant session key and expiry between views and controllers. Not mapped or serialized with the object.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public SagePayMerchantSessionKey MerchantSession { get; set; }

    }
}
