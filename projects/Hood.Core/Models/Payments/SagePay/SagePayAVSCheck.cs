namespace Hood.Models.Payments
{
    public class SagePayAVSCheck
    {
        public string Status { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string SecurityCode { get; set; }
    }
}
