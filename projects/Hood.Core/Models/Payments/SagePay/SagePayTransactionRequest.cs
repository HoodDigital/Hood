using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models.Payments
{
    public partial class SagePayTransactionRequest : SagePayTransactionBase
    {
        [NotMapped]
        public SagePayPaymentMethodRequest PaymentMethod { get; set; }

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
