using Hood.Extensions;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;

namespace Hood.Models
{
    public partial class Auth0User : IHoodIdentity
    {
        /// <summary>
        ///     Constructor which creates a new Guid for the Id
        /// </summary>
        public Auth0User()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     Constructor that takes a userName
        /// </summary>
        /// <param name="userName"></param>
        public Auth0User(string userName)
            : this()
        {
            UserName = userName;
        }

        public UserProfile UserProfile { get; set; }
        public IList<Auth0Identity> ConnectedAuth0Accounts { get; set; }
        [PersonalData]
        public string Id { get; set; }
        [ProtectedPersonalData]
        public string UserName { get; set; }
        [PersonalData]
        public bool EmailConfirmed { get; set; }
        [ProtectedPersonalData]
        public string Email { get; set; }
        [PersonalData]
        public bool PhoneNumberConfirmed { get; set; }
        [ProtectedPersonalData]
        public string PhoneNumber { get; set; }

        public bool Active { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastLogOn { get; set; }
        public string LastLoginIP { get; set; }
        public string LastLoginLocation { get; set; }
        public int AccessFailedCount { get; set; }
        [PersonalData]
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }

        public Auth0Identity GetPrimaryIdentity()
        {
            if (ConnectedAuth0Accounts == null || ConnectedAuth0Accounts.Count == 0)
            {
                return null;
            }
            return ConnectedAuth0Accounts.FirstOrDefault(ca => ca.IsPrimary);
        }

        public bool UpdateFromPrincipal(ClaimsPrincipal principal)
        {
            bool changed = false;

            string firstName = principal.GetClaimValue(ClaimTypes.GivenName);
            if (!this.UserProfile.FirstName.IsSet() && firstName.IsSet())
            {
                this.UserProfile.FirstName = firstName;
                changed = true;
            }

            string lastName = principal.GetClaimValue(ClaimTypes.Surname);
            if (!this.UserProfile.LastName.IsSet() && lastName.IsSet())
            {
                this.UserProfile.LastName = lastName;
                changed = true;
            }

            string mobile = principal.GetClaimValue(ClaimTypes.MobilePhone);
            if (!this.PhoneNumber.IsSet() && mobile.IsSet())
            {
                this.PhoneNumber = mobile;
                changed = true;
            }

            bool emailConfirmed = principal.IsEmailConfirmed();
            if (!this.EmailConfirmed && emailConfirmed)
            {
                this.EmailConfirmed = emailConfirmed;
                changed = true;
            }

            string avatar = principal.GetAvatar();
            if (!this.UserProfile.AvatarJson.IsSet() && avatar.IsSet())
            {
                this.UserProfile.Avatar = new MediaObject(avatar);
                changed = true;
            }

            return changed;
        }
    }


}
