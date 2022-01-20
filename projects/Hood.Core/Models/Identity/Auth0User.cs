using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Hood.Entities;
using Newtonsoft.Json;

namespace Hood.Models
{
    public class Auth0UserList
    {
        [JsonProperty("users")]
        public List<Auth0User> Users { get; set; }
        [JsonProperty("start")]
        public int Start { get; set; }
        [JsonProperty("limit")]
        public int Limit { get; set; }
        [JsonProperty("length")]
        public int Length { get; set; }
        [JsonProperty("total")]
        public int Total { get; set; }

    }
    public class Auth0User : BaseEntity<string>
    {
        [JsonProperty("user_id")]
        public override string Id { get; set; }

        [JsonIgnore]
        public string UserId { get; set; }
        [JsonIgnore]
        public ApplicationUser User { get; set; }


        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("email_verified")]
        public bool EmailVeriried { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }
        [JsonProperty("phone_verified")]
        public bool PhoneVerified { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty("app_metadata")]
        [NotMapped]
        public Dictionary<string, string> AppMetadata { get; set; }
        [JsonProperty("user_metadata")]
        [NotMapped]
        public Dictionary<string, string> UserMetadata { get; set; }
        [JsonProperty("picture")]
        public string PictureUrl { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("nickname")]
        public string DisplayName { get; set; }
        [JsonProperty("last_ip")]
        public string LastLoginIp { get; set; }
        [JsonProperty("last_login")]
        public DateTime LastLogin { get; set; }
        [JsonProperty("logins_count")]
        public int LoginCount { get; set; }
        [JsonProperty("blocked")]
        public bool Blocked { get; set; }
        [JsonProperty("given_name")]
        public string FirstName { get; set; }
        [JsonProperty("family_name")]
        public string LastName { get; set; }
    }
}