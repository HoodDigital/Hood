using Hood.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{
    /// <summary>
    /// Represents a role in the identity system
    /// </summary>
    public class Auth0Role
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Auth0Role"/>.
        /// </summary>
        public Auth0Role() { }

        /// <summary>
        /// Initializes a new instance of <see cref="Auth0Role"/>.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        public Auth0Role(string roleName) : this()
        {
            Name = roleName;
        }

        /// <summary>
        /// Gets or sets the primary key for this role.
        /// </summary>
        public virtual string Id { get; set; } = default!;

        /// <summary>
        /// Gets or sets the name for this role.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the normalized name for this role.
        /// </summary>
        public virtual string NormalizedName { get; set; }

        /// <summary>
        /// A random value that should change whenever a role is persisted to the store
        /// </summary>
        public virtual string ConcurrencyStamp { get; set; }

        /// <summary>
        /// Returns the name of the role.
        /// </summary>
        /// <returns>The name of the role.</returns>
        public override string ToString()
        {
            return Name ?? string.Empty;
        }
    }


    /// <summary>
    /// Represents the link between a user and a role.
    /// </summary>
    public class Auth0UserRole
    {
        /// <summary>
        /// Gets or sets the primary key of the user that is linked to a role.
        /// </summary>
        public virtual string UserId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the primary key of the role that is linked to the user.
        /// </summary>
        public virtual string RoleId { get; set; } = default!;
    }

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

        public virtual ICollection<Auth0UserRole> Roles { get; set; }

        public string Id { get; set; }
        public string UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Email { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public string BillingAddressJson { get; set; }
        public string DeliveryAddressJson { get; set; }
        public string AvatarJson { get; set; }
        public UserProfile UserProfile { get; set; }

        public IList<Auth0Identity> ConnectedAuth0Accounts { get; set; }

        public bool Active { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastLogOn { get; set; }
        public string LastLoginIP { get; set; }
        public string LastLoginLocation { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public Auth0Identity GetPrimaryIdentity()
        {
            if (ConnectedAuth0Accounts == null || ConnectedAuth0Accounts.Count == 0)
            {
                return null;
            }
            return ConnectedAuth0Accounts.SingleOrDefault(ca => ca.IsPrimary);
        }
    }


}
