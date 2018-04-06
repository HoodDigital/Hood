using Hood.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Hood.Models.Payments
{
    public partial class SagePayTransaction : SagePayTransactionBase
    {
        #region "Response"

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

        #endregion

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
                        CardType = _CardType,
                        Reusable = _Reusable,
                        ExpiryDate = _ExpiryDate,
                        LastFourDigits = _LastFourDigits
                    }
                };
            }
            set
            {
                _CardIdentifier = value.Card.CardIdentifier;
                _CardType = value.Card.CardType;
                _Reusable = value.Card.Reusable;
                _ExpiryDate = value.Card.ExpiryDate;
                _LastFourDigits = value.Card.LastFourDigits;
            }
        }

        [NonSerialized]
        private string _CardIdentifier;
        [NonSerialized]
        private string _CardType;
        [NonSerialized]
        private bool _Reusable;
        [NonSerialized]
        private string _ExpiryDate;
        [NonSerialized]
        private string _LastFourDigits;

        /// <summary>
        ///  The currency of the amount in 3 letter ISO 4217 format. e.g. GBP for Pound Sterling.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Provides information regarding the transaction amount. This is only returned in GET requests.
        /// </summary>
        public SagePayAmount Amount { get; set; }

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

        /// <summary>
        /// Provides information regarding the AVS/CV2 check results.
        /// </summary>
        [NotMapped]
        public SagePayAVSCheck AvsCvcCheck { get; set; }

        /// <summary>
        /// Provides information regarding the AVS/CV2 check results.
        /// </summary>
        [NotMapped]
        [JsonProperty(PropertyName = "3DSecure")]
        public SagePay3DSecureCheck Response3DSecure { get; set; }

    }
}
