namespace Hood.Models.Payments
{
    public partial class SagePayCardRequest
    {
        /// <summary>
        /// The merchant session key used to generate the cardIdentifier.
        /// </summary>
        public string MerchantSessionKey { get; set; }

        /// <summary>
        /// The unique reference of the card you want to charge.
        /// </summary>
        public string CardIdentifier { get; set; }

        /// <summary>
        ///  A flag to indicate the card identifier is reusable, i.e. it has been created previously. 
        /// </summary>
        public bool Reusable { get; set; }

        /// <summary>
        /// A flag to indicate that you want to save the card identifier, i.e. make it reusable. 
        /// </summary>
        public bool Save { get; set; }
    }
}
