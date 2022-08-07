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
    public class Auth0Identity
    {
        public Auth0Identity()
        { }

        public Auth0Identity(Auth0.ManagementApi.Models.Identity identity)
        {
            Id = identity.UserId;
            Provider = identity.Provider;
        }

        [Key]
        [JsonIgnore]
        public string Id { get; set; }
        
        [JsonIgnore]
        public string UserId { get; set; }
        [JsonIgnore]
        public Auth0User User { get; set; }
        
        [JsonIgnore]
        public bool IsPrimary { get; set; }
        public string Picture { get; set; }
        public string Provider { get; set; }

        public string ToProviderIcon()
        {
            switch (Provider)
            {
                case "email":
                    return "<i class='fa fa-envelope me-2'></i>";

                case "google-oauth2":
                    return "<i class='fab fa-google me-2'></i>";

                case "auth0":
                    return "<i class='fa fa-lock me-2'></i>";

                default:
                    return "<i class='fa fa-external-link me-2'></i>Extenal";
            }
        }
        public string ToProviderString()
        {
            switch (Provider)
            {
                case "email":
                    return "Passwordless (E-Mail)";

                case "google-oauth2":
                    return "Google";

                case "auth0":
                    return "Password";

                default:
                    return "External";
            }
        }


    }
}