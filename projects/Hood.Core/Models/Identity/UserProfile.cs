using Hood.Extensions;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
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
        public string RolesJson { get; set; }
        [NotMapped]
        public List<ApplicationRole> Roles
        {
            get { return !RolesJson.IsSet() ? new List<ApplicationRole>() : JsonConvert.DeserializeObject<List<ApplicationRole>>(RolesJson); }
            set { RolesJson = JsonConvert.SerializeObject(value); }
        }
        #endregion

        #region Auth0 
        public string Auth0UsersJson { get; set; }
        [NotMapped]
        public List<Auth0User> Auth0Users
        {
            get { return !Auth0UsersJson.IsSet() ? new List<Auth0User>() : JsonConvert.DeserializeObject<List<Auth0User>>(Auth0UsersJson); }
            set { Auth0UsersJson = JsonConvert.SerializeObject(value); }
        }
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
