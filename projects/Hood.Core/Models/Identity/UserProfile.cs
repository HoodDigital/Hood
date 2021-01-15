using Hood.Extensions;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{
    public class UserProfile : UserProfileBase
    {
        #region Roles
        public string RoleIds { get; set; }

        public int RoleCount { get; set; }
        internal string RolesJson { get; set; }
        public List<IdentityRole> Roles
        {
            get { return !RolesJson.IsSet() ? new List<IdentityRole>() : JsonConvert.DeserializeObject<List<IdentityRole>>(RolesJson); }
            set { RolesJson = JsonConvert.SerializeObject(value); }
        }
        #endregion

        #region View Model Stuff
        [NotMapped]
        public IList<IdentityRole> AllRoles { get; set; }
        [NotMapped]
        public Customer Customer { get; set; }
        [NotMapped]
        public List<Customer> MatchedCustomerObjects { get; set; }
        #endregion

        #region Prevent Mapping Sensitive Fields
        [NotMapped]
        public override DateTimeOffset? LockoutEnd { get; set; }
        [NotMapped]
        public override string ConcurrencyStamp { get; set; }
        [NotMapped]
        public override string SecurityStamp { get; set; }
        [NotMapped]
        public override string PasswordHash { get; set; }
        [NotMapped]
        public override string NormalizedEmail { get; set; }
        [NotMapped]
        public override string NormalizedUserName { get; set; }
        [NotMapped]
        public override int AccessFailedCount { get; set; }
        #endregion
    }
}
