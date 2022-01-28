using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Auth0.ManagementApi.Models;
using Hood.Entities;
using Hood.Extensions;
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
    public class Auth0User : Auth0.ManagementApi.Models.User
    {
        public Auth0User()
        { }

        public Auth0User(User user)
        {
            user.CopyProperties(this);
        }

        [JsonIgnore]
        public string LocalUserId { get; set; }
        [JsonIgnore]
        public ApplicationUser User { get; set; }
        [JsonIgnore]
        public string ProviderName { get; set; }
        public string ToProviderString()
        {
            switch (ProviderName)
            {
                case "email":
                    return "<i class='fa fa-envelope me-2'></i>Passwordless (E-Mail)";

                case "google-oauth2":
                    return "<i class='fab fa-google me-2'></i>Google";

                case "auth0":
                    return "<i class='fa fa-lock me-2'></i>Password";

                default:
                    return "<i class='fa fa-external-link me-2'></i>External";
            }            
        }

    }
}