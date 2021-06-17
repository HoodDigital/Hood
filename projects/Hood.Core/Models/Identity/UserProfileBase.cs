using Hood.Attributes;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public abstract class UserProfileBase : IdentityUser, IUserProfile
    {

        #region LogOn Information
        [Display(Name = "Account Activated")]
        public virtual bool Active { get; set; }
        public virtual DateTime CreatedOn { get; set; }
        public virtual DateTime LastLogOn { get; set; }
        [ProtectedPersonalData]
        public virtual string LastLoginIP { get; set; }
        [ProtectedPersonalData]
        public virtual string LastLoginLocation { get; set; }
        [ProtectedPersonalData]
        public virtual string Latitude { get; set; }
        [ProtectedPersonalData]
        public virtual string Longitude { get; set; }
        [NotMapped]
        [JsonIgnore]
        public List<UserAccessCode> AccessCodes { get; set; }
        #endregion

        #region IName 

        [FormUpdatable]
        [Display(Name = "Anonymous", Description = "If set, user will details will not show up in public areas of the site.")]
        public virtual bool Anonymous { get; set; }

        [NotMapped]
        public virtual string FullName { get; set; }

        [FormUpdatable]
        [Display(Name = "First name")]
        [ProtectedPersonalData]
        public virtual string FirstName { get; set; }

        [FormUpdatable]
        [Display(Name = "Last name")]
        [ProtectedPersonalData]
        public virtual string LastName { get; set; }

        [FormUpdatable]
        [ProtectedPersonalData]
        [Display(Name = "Display name")]
        public virtual string DisplayName { get; set; }
  
        
        public virtual string ToAdminName()
        {
            if (this.ToFullName().IsSet())
                return this.ToFullName();
            if (UserName.IsSet())
                return UserName;
            if (Email.IsSet())
                return Email;
            else return "";
        }
        #endregion

        #region Addresses 
        public virtual List<Address> Addresses { get; set; }

        public virtual string BillingAddressJson { get; set; }
        [NotMapped]
        public virtual Address DeliveryAddress
        {
            get { return DeliveryAddressJson.IsSet() ? JsonConvert.DeserializeObject<Address>(DeliveryAddressJson) : null; }
            set { DeliveryAddressJson = JsonConvert.SerializeObject(value); }
        }

        public virtual string DeliveryAddressJson { get; set; }
        [NotMapped]
        public virtual Address BillingAddress
        {
            get { return BillingAddressJson.IsSet() ? JsonConvert.DeserializeObject<Address>(BillingAddressJson) : null; }
            set { BillingAddressJson = JsonConvert.SerializeObject(value); }
        }

        #endregion

        #region Avatar
        public virtual string AvatarJson { get; set; }
        [NotMapped]
        public virtual IMediaObject Avatar
        {
            get { return AvatarJson.IsSet() ? JsonConvert.DeserializeObject<MediaObject>(AvatarJson) : MediaObject.Blank; }
            set { AvatarJson = JsonConvert.SerializeObject(value); }
        }
        #endregion

        #region Metadata
        public virtual string UserVars { get; set; }
        [NotMapped]
        public virtual Dictionary<string, string> Metadata
        {
            get { return UserVars.IsSet() ? JsonConvert.DeserializeObject<Dictionary<string, string>>(UserVars) : new Dictionary<string, string>(); }
            set { UserVars = JsonConvert.SerializeObject(value); }
        }
        public virtual string this[string key]
        {
            get
            {
                if (Metadata.ContainsKey(key))
                    return Metadata[key];
                return null;
            }
            set
            {
                var list = Metadata;
                if (list.ContainsKey(key))
                    list[key] = value;
                else
                    list.Add(key, value);
                Metadata = list;
            }
        }
        #endregion

        #region GDPR 
        [NotMapped]
        [PersonalData]
        [Display(Name = "Marketing Emails", Description = "Whether or not you consent to marketing emails.")]
        public virtual bool MarketingEmails
        {
            get { return this[nameof(MarketingEmails)] != null ? JsonConvert.DeserializeObject<bool>(this[nameof(MarketingEmails)]) : false; }
            set { this[nameof(MarketingEmails)] = JsonConvert.SerializeObject(value); }
        }
        #endregion

        #region Socials 

        [FormUpdatable]
        [NotMapped]
        [PersonalData]
        [Display(Name = "Website URL")]
        public virtual string WebsiteUrl { get => this[nameof(WebsiteUrl)]; set => this[nameof(WebsiteUrl)] = value; }

        [FormUpdatable]
        [NotMapped]
        [PersonalData]
        [Display(Name = "Twitter URL")]
        public virtual string Twitter { get => this[nameof(Twitter)]; set => this[nameof(Twitter)] = value; }

        [FormUpdatable]
        [NotMapped]
        [PersonalData]
        [Display(Name = "Twitter Handle (@yourname)")]
        public virtual string TwitterHandle { get => this[nameof(TwitterHandle)]; set => this[nameof(TwitterHandle)] = value; }

        [FormUpdatable]
        [NotMapped]
        [PersonalData]
        [Display(Name = "Facebook URL")]
        public virtual string Facebook { get => this[nameof(Facebook)]; set => this[nameof(Facebook)] = value; }

        [FormUpdatable]
        [NotMapped]
        [PersonalData]
        [Display(Name = "Instagram URL")]
        public virtual string Instagram { get => this[nameof(Instagram)]; set => this[nameof(Instagram)] = value; }

        [FormUpdatable]
        [NotMapped]
        [PersonalData]
        [Display(Name = "LinkedIn URL")]
        public virtual string LinkedIn { get => this[nameof(LinkedIn)]; set => this[nameof(LinkedIn)] = value; }
        #endregion

        #region Extra Profile Fields

        [FormUpdatable]
        [NotMapped]
        [PersonalData]
        [Display(Name = "Company name")]
        public virtual string CompanyName { get => this[nameof(CompanyName)]; set => this[nameof(CompanyName)] = value; }

        [FormUpdatable]
        [NotMapped]
        [PersonalData]
        [Display(Name = "Bio")]
        public virtual string Bio { get => this[nameof(Bio)]; set => this[nameof(Bio)] = value; }

        [FormUpdatable]
        [NotMapped]
        [PersonalData]
        [Display(Name = "Job Title")]
        public virtual string JobTitle { get => this[nameof(JobTitle)]; set => this[nameof(JobTitle)] = value; }
        #endregion

        #region Notes 
       
        [NotMapped]
        public virtual List<UserNote> Notes
        {
            get { return this[nameof(Notes)] != null ? JsonConvert.DeserializeObject<List<UserNote>>(this[nameof(Notes)]) : new List<UserNote>(); }
            set { this[nameof(Notes)] = JsonConvert.SerializeObject(value); }
        }
   
        public virtual void AddUserNote(UserNote note)
        {
            var notes = this.Notes;
            notes.Add(note);
            this.Notes = notes;
        }
  
        #endregion

        #region ISaveableModel
        [NotMapped]
        [JsonIgnore]
        public virtual AlertType MessageType { get; set; }
        [NotMapped]
        [JsonIgnore]
        public virtual string SaveMessage { get; set; }
        #endregion

        #region View Model Stuff
        [NotMapped]
        public IList<IdentityRole> AllRoles { get; set; }
        #endregion

    }
}