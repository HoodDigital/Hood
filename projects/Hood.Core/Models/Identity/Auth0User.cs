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
        {}
        
        public Auth0User(User user)
        {
            user.CopyProperties(this);
        }

        [JsonIgnore]
        public string LocalUserId { get; set; }
        [JsonIgnore]
        public ApplicationUser User { get; set; }
    }
}