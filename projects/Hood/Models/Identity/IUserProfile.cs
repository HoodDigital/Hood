using Hood.Interfaces;
using System;
using System.Collections.Generic;

namespace Hood.Models
{
    /// <summary>
    /// IUserProfile is used for setting the user's profile and passing data to views, while excluding sensetive data from the IdentityUser classes.
    /// </summary>
    public interface IUserProfile : IName, IJsonMetadata, IAvatar, ISaveableModel, ILoginInformation
    {
        string Id { get; set; }

        #region Socials 
        string WebsiteUrl { get; set; }
        string Twitter { get; set; }
        string TwitterHandle { get; set; }
        string Facebook { get; set; }
        string Instagram { get; set; }
        string LinkedIn { get; set; }
        #endregion

        #region Extra Profile Fields
        string ForumSignature { get; set; }
        string CompanyName { get; set; }
        string Bio { get; set; }
        string JobTitle { get; set; }
        #endregion

        #region Notes 
        List<UserNote> Notes { get; set; }
        void AddUserNote(UserNote note);
        string ToAdminName();
        #endregion
    }

    public interface ILoginInformation
    {
        #region Identity
        /// <summary>
        /// Gets or sets the user name for this user.
        /// </summary>
        string UserName { get; set; }
        /// <summary>
        /// Gets or sets a flag indicating if a user has confirmed their email address.
        /// </summary>
        bool EmailConfirmed { get; set; }
        /// <summary>
        /// Gets or sets the email address for this user.
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if a user has confirmed their telephone address.
        /// </summary>
        bool PhoneNumberConfirmed { get; set; }
        /// <summary>
        /// Gets or sets a telephone number for the user.
        /// </summary>
        // Summary:
        string PhoneNumber { get; set; }

        #endregion

        #region LogOn Information
        bool Active { get; set; }
        DateTime CreatedOn { get; set; }
        DateTime LastLogOn { get; set; }
        string LastLoginIP { get; set; }
        string LastLoginLocation { get; set; }
        string Latitude { get; set; }
        string Longitude { get; set; }
        #endregion


    }
}