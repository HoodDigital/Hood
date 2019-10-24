using Hood.Extensions;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

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
            get { return _PaymentMethodJson.IsSet() ? JsonConvert.DeserializeObject<SagePayPaymentMethod>(_PaymentMethodJson) : null; }
            set { _PaymentMethodJson = JsonConvert.SerializeObject(value); }
        }

        [NonSerialized]
        private string _PaymentMethodJson;

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
            get { return _AmountJson.IsSet() ? JsonConvert.DeserializeObject<SagePayAmount>(_AmountJson) : null; }
            set { _AmountJson = JsonConvert.SerializeObject(value); }
        }

        [NonSerialized]
        private string _AmountJson;

        /// <summary>
        /// Provides information regarding the AVS/CV2 check results.
        /// </summary>
        [NotMapped]
        public SagePayAVSCheck AvsCvcCheck
        {
            get { return _AvsCvcCheckJson.IsSet() ? JsonConvert.DeserializeObject<SagePayAVSCheck>(_AvsCvcCheckJson) : null; }
            set { _AvsCvcCheckJson = JsonConvert.SerializeObject(value); }
        }

        [NonSerialized]
        private string _AvsCvcCheckJson;

        /// <summary>
        /// Provides information regarding the AVS/CV2 check results.
        /// </summary>
        [NotMapped]
        [JsonProperty(PropertyName = "3DSecure")]
        public SagePay3DSecureCheck Response3DSecure
        {
            get { return _Response3DSecure.IsSet() ? JsonConvert.DeserializeObject<SagePay3DSecureCheck>(_Response3DSecure) : null; }
            set { _Response3DSecure = JsonConvert.SerializeObject(value); }
        }

        [NonSerialized]
        private string _Response3DSecure;
    }
}
