using System;

namespace Hood.Models.Payments
{
    public class OrderNote
    {
        public DateTime CreatedOn { get; set; }
        public string UserId { get; set; }
        public string Note { get; set; }
    }
}