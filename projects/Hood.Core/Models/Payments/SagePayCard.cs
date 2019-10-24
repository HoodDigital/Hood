namespace Hood.Models.Payments
{
    public partial class SagePayCard
    {
        /// <summary>
        /// The unique reference of the card you want to charge.
        /// </summary>
        public string CardIdentifier { get; set; }

        /// <summary>
        ///  A flag to indicate the card identifier is reusable, i.e. it has been created previously. 
        /// </summary>
        public bool Reusable { get; set; }

        /// <summary>
        /// The type of the card (Visa, MasterCard, American Express etc.).
        /// </summary>
        public string CardType { get; set; }

        /// <summary>
        /// The last 4 digits of the card.
        /// </summary>
        public string LastFourDigits { get; set; }

        /// <summary>
        /// The expiry date of the card in MMYY format.
        /// </summary>
        public string ExpiryDate { get; set; }

    }
}
