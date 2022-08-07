using System;
using System.Collections.Generic;

namespace Hood.Models
{
    public interface IHoodIdentity
    {
        string Id { get; set; }
        string UserName { get; set; }
        bool EmailConfirmed { get; set; }
        string Email { get; set; }
        bool PhoneNumberConfirmed { get; set; }
        bool Active { get; set; }
        string PhoneNumber { get; set; }
        string BillingAddressJson { get; set; }
        string DeliveryAddressJson { get; set; }
        DateTime CreatedOn { get; set; }
        DateTime LastLogOn { get; set; }
        string LastLoginIP { get; set; }
        string LastLoginLocation { get; set; }
    }
}