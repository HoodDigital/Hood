using Hood.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Hood.Models.Payments
{
    public partial class SagePayTransaction : BaseOrder<SagePayAddress>
    {
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
        /// Use this field to override your default account level 3-D Secure settings.
        /// <para>UseMSPSetting - Use default MySagePay settings.</para>
        /// <para>Force - Apply authentication even if turned off. </para>
        /// <para>Disable - Disable authentication and rules.</para>
        /// <para>ForceIgnoringRules - Apply authentication but ignore rules.</para>
        /// </summary>
        public string Apply3DSecure { get; set; }

        /// <summary>
        ///  Use this field to override your default account level AVS CVC settings. 
        /// <para>UseMSPSetting - Use default MySagePay settings.</para>
        /// <para>Force - Apply authentication even if turned off. </para>
        /// <para>Disable - Disable authentication and rules.</para>
        /// <para>ForceIgnoringRules - Apply authentication but ignore rules.</para>
        /// </summary>
        public string ApplyAvsCvcCheck { get; set; }

        /// <summary>
        /// Provides information regarding the AVS/CV2 check results.
        /// </summary>
        [NotMapped]
        public SagePayAVSCheck AvsCvcCheck { get; set; }

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
        /// Result of transaction registration. 
        /// <para>Ok - Transaction request completed successfully.</para> 
        /// <para>NotAuthed - Transaction request was not authorised by the bank.</para> 
        /// <para>Rejected - Transaction rejected by your fraud rules.</para> 
        /// <para>Malformed - Missing properties or badly formed body.</para> 
        /// <para>Invalid - Invalid property values supplied.</para> 
        /// <para>Error - An error occurred at Sage Pay.</para> 
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Code related to the status of the transaction. Successfully authorised transactions will have the statusCode of 0000. 
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// A detailed reason for the status of the transaction.
        /// </summary>
        public string StatusDetail { get; set; }

        /// <summary>
        /// Sage Pay unique Authorisation Code for a successfully authorised transaction. Only present if status is Ok.
        /// </summary>
        public string RetrievalReference { get; set; }

        /// <summary>
        /// Also known as the decline code, these are codes that are specific to your merchant bank. Please contact them for a description of each code. Only returned with transaction type 'Payment'.
        /// </summary>
        public string BankResponseCode { get; set; }

        /// <summary>
        /// The authorisation code returned from your merchant bank.
        /// </summary>
        public string BankAuthorisationCode { get; set; }

        #region "Payment Method Object"

        /// <summary>
        /// Payment method for this transaction.
        /// </summary>
        [NotMapped]
        public SagePayPaymentMethod PaymentMethod
        {
            get
            {
                return new SagePayPaymentMethod()
                {
                    Card = new SagePayCard()
                    {
                        CardIdentifier = _CardIdentifier,
                        MerchantSessionKey = _MerchantSessionKey,
                        Reusable = _Reusable,
                        Save = _Save
                    }
                };
            }
            set
            {
                _CardIdentifier = value.Card.CardIdentifier;
                _MerchantSessionKey = value.Card.MerchantSessionKey;
                _Reusable = value.Card.Reusable;
                _Save = value.Card.Save;
            }
        }

        /// <summary>
        /// The merchant session key used to generate the cardIdentifier.
        /// </summary>
        [NonSerialized]
        private string _MerchantSessionKey;

        /// <summary>
        /// The unique reference of the card you want to charge.
        /// </summary>
        [NonSerialized]
        private string _CardIdentifier;

        /// <summary>
        ///  A flag to indicate the card identifier is reusable, i.e. it has been created previously. 
        /// </summary>
        [NonSerialized]
        private bool _Reusable;

        /// <summary>
        /// A flag to indicate that you want to save the card identifier, i.e. make it reusable. 
        /// </summary>
        [NonSerialized]
        private bool _Save;

        [NotMapped]
        [JsonIgnore]
        public SagePayMerchantSessionKey MerchantSession { get; set; }

        #endregion

        #region "Amount"

        /// <summary>
        ///  The currency of the amount in 3 letter ISO 4217 format. e.g. GBP for Pound Sterling.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Provides information regarding the transaction amount. This is only returned in GET requests.
        /// </summary>
        [NotMapped]
        public SagePayAmount Amount
        {
            get
            {
                return new SagePayAmount()
                {
                    SaleAmount = _SaleAmount,
                    SurchargeAmount = _SurchargeAmount,
                    TotalAmount = _TotalAmount
                };
            }
            set
            {
                _SaleAmount = value.SaleAmount;
                _SurchargeAmount = value.SurchargeAmount;
                _TotalAmount = value.TotalAmount;
            }
        }

        /// <summary>
        /// The total amount for the transaction that includes any sale or surcharge values.
        /// </summary>
        [NonSerialized]
        private int _TotalAmount;

        /// <summary>
        /// The sale amount associated with the cost of goods or services for the transaction.
        /// </summary>
        [NonSerialized]
        private int _SaleAmount;

        /// <summary>
        /// The surcharge amount added to the transaction as per the settings of the account.
        /// </summary>
        [NonSerialized]
        private int _SurchargeAmount;

        #endregion
    }
}
