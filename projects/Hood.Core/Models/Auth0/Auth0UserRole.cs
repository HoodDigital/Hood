namespace Hood.Models
{
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


}
