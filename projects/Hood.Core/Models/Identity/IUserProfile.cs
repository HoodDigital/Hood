using Hood.Interfaces;
using System;
using System.Collections.Generic;

namespace Hood.Models
{
    public interface IUserProfile : IName, IJsonMetadata, IAvatar, ISaveableModel, IHoodIdentity
    {
        #region Socials 
        string WebsiteUrl { get; set; }
        string Twitter { get; set; }
        string TwitterHandle { get; set; }
        string Facebook { get; set; }
        string Instagram { get; set; }
        string LinkedIn { get; set; }
        #endregion

        #region Extra Profile Fields
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
}