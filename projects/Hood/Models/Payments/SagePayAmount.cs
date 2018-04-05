namespace Hood.Models.Payments
{
    public class SagePayAmount
    {
        /// <summary>
        /// The total amount for the transaction that includes any sale or surcharge values.
        /// </summary>
        public int TotalAmount { get; set; }

        /// <summary>
        /// The sale amount associated with the cost of goods or services for the transaction.
        /// </summary>
        public int SaleAmount { get; set; }

        /// <summary>
        /// The surcharge amount added to the transaction as per the settings of the account.
        /// </summary>
        public int SurchargeAmount { get; set; }
    }
}
