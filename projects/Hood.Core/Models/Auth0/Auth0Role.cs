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


}
