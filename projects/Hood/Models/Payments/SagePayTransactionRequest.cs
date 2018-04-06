using Hood.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Hood.Models.Payments
{
    public partial class SagePayTransactionRequest : SagePayTransactionBase
    {
        [NotMapped]
        [JsonIgnore]
        public SagePayMerchantSessionKey MerchantSession { get; set; }

        [NotMapped]
        public SagePayPaymentMethodRequest PaymentMethod { get; set; }

        /// <summary>
        ///  The currency of the amount in 3 letter ISO 4217 format. e.g. GBP for Pound Sterling.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Provides information regarding the transaction amount. This is only returned in GET requests.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        ///  Use this field to override your default account level AVS CVC settings. 
        /// <para>UseMSPSetting - Use default MySagePay settings.</para>
        /// <para>Force - Apply authentication even if turned off. </para>
        /// <para>Disable - Disable authentication and rules.</para>
        /// <para>ForceIgnoringRules - Apply authentication but ignore rules.</para>
        /// </summary>
        public string ApplyAvsCvcCheck { get; set; }
        
        /// <summary>
        /// Use this field to override your default account level 3-D Secure settings.
        /// <para>UseMSPSetting - Use default MySagePay settings.</para>
        /// <para>Force - Apply authentication even if turned off. </para>
        /// <para>Disable - Disable authentication and rules.</para>
        /// <para>ForceIgnoringRules - Apply authentication but ignore rules.</para>
        /// </summary>
        public string Apply3DSecure { get; set; }

    }
}
