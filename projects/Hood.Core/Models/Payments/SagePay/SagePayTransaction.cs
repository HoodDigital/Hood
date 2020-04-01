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
        /// Provides information regarding the AVS/CV2 check results.
        /// </summary>
        [NotMapped]
        public SagePayAVSCheck AvsCvcCheck
        {
            get { return AvsCvcCheckJson.IsSet() ? JsonConvert.DeserializeObject<SagePayAVSCheck>(AvsCvcCheckJson) : null; }
            set { AvsCvcCheckJson = JsonConvert.SerializeObject(value); }
        }

        public string AvsCvcCheckJson { get; set; }

        /// <summary>
        /// Provides information regarding the AVS/CV2 check results.
        /// </summary>
        [NotMapped]
        [JsonProperty(PropertyName = "3DSecure")]
        public SagePay3DSecureCheck Response3DSecure
        {
            get { return Response3DSecureJson.IsSet() ? JsonConvert.DeserializeObject<SagePay3DSecureCheck>(Response3DSecureJson) : null; }
            set { Response3DSecureJson = JsonConvert.SerializeObject(value); }
        }

        public string Response3DSecureJson { get; set; }
    }
}
