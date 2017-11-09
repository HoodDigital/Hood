using Hood.Models;
using System;
using System.Collections.Generic;

namespace Hood.Interfaces
{
    public interface IHoodUser
    {
        DateTimeOffset? LockoutEnd { get; set; }
        bool TwoFactorEnabled { get; set; }
        bool PhoneNumberConfirmed { get; set; }
        string PhoneNumber { get; set; }
        string ConcurrencyStamp { get; set; }
        string SecurityStamp { get; set; }
        string PasswordHash { get; set; }
        bool EmailConfirmed { get; set; }
        string NormalizedEmail { get; set; }
        string Email { get; set; }
        string NormalizedUserName { get; set; }
        string UserName { get; set; }
        string Id { get; set; }
        bool LockoutEnabled { get; set; }
        int AccessFailedCount { get; set; }
        bool Active { get; set; }
        string AvatarJson { get; set; }
        string BillingAddressJson { get; set; }
        string Bio { get; set; }
        string ClientCode { get; set; }
        string CompanyName { get; set; }
        DateTime CreatedOn { get; set; }
        string DeliveryAddressJson { get; set; }
        string DisplayName { get; set; }
        bool EmailOptin { get; set; }
        string Facebook { get; set; }
        string FirstName { get; set; }
        string FullName { get; }
        string GooglePlus { get; set; }
        string JobTitle { get; set; }
        string LastLoginIP { get; set; }
        string LastLoginLocation { get; set; }
        DateTime LastLogOn { get; set; }
        string LastName { get; set; }
        string Latitude { get; set; }
        string LinkedIn { get; set; }
        string Longitude { get; set; }
        string Mobile { get; set; }
        string Notes { get; set; }
        string Phone { get; set; }
        string StripeId { get; set; }
        string SystemNotes { get; set; }
        string Twitter { get; set; }
        string TwitterHandle { get; set; }
        Dictionary<string, string> UserVariables { get; set; }
        string UserVars { get; set; }
        string VATNumber { get; set; }
        string WebsiteUrl { get; set; }
        string ReplacePlaceholders(string msg);
        SiteMedia Avatar { get; set; }
    }
}