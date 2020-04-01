using Hood.Interfaces;
using Newtonsoft.Json;

namespace Hood.Models.Payments
{
    public class SagePayAddress : OrderAddress
    {
        public string RecipientFirstName { get; set; }
        public string RecipientLastName { get; set; }
    }
}
