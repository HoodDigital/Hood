using Hood.Entities;
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
        /// Provides information regarding the transaction amount. This is only returned in GET requests.
        /// </summary>
        public string Amount { get; set; }
        public string Currency { get; set; }

        /// <summary>
        /// A brief description of the goods or services purchased.
        /// </summary>
        public string Description { get; set; }

        public string GiftAid { get; set; }
        public string Apply3DSecure => "Disable";
        public string ApplyAvsCvcCheck => "UseMPSetting";
        
        [NotMapped]
        public SagePayAVSCheck AvsCvcCheck { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

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
        private string _MerchantSessionKey;

        /// <summary>
        /// The unique reference of the card you want to charge.
        /// </summary>
        private string _CardIdentifier;

        /// <summary>
        ///  A flag to indicate the card identifier is reusable, i.e. it has been created previously. 
        /// </summary>
        private bool _Reusable;

        /// <summary>
        /// A flag to indicate that you want to save the card identifier, i.e. make it reusable. 
        /// </summary>
        private bool _Save;

        #endregion
    }
}
