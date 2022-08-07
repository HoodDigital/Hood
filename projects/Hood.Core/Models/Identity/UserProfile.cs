using Hood.Attributes;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Hood.Models
{
    public class UserProfileView<TRole> : UserProfile
    {
        public string RoleIds { get; set; }
        public int RoleCount { get; set; }
        public string RolesJson { get; set; }
        [NotMapped]
        public List<TRole> Roles
        {
            get { return !RolesJson.IsSet() ? new List<TRole>() : JsonConvert.DeserializeObject<List<TRole>>(RolesJson); }
            set { RolesJson = JsonConvert.SerializeObject(value); }
        }
        [NotMapped]
        public List<TRole> AllRoles { get; set; }
    }
    public class UserProfile : IUserProfile, IName
    {
        public virtual string Id { get; set; }

        public virtual string UserName { get; set; }
        public virtual bool EmailConfirmed { get; set; }
        public virtual string Email { get; set; }
        public virtual bool PhoneNumberConfirmed { get; set; }
        public virtual string PhoneNumber { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastLogOn { get; set; }
        public string LastLoginIP { get; set; }
        public string LastLoginLocation { get; set; }

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
            if (this.ToInternalName().IsSet())
                return this.ToInternalName();
            if (UserName.IsSet())
                return UserName;
            if (Email.IsSet())
                return Email;
            else return "";
        }
        #endregion

        #region Addresses   
              
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
            get { return AvatarJson.IsSet() ? JsonConvert.DeserializeObject<MediaObject>(AvatarJson) : MediaObject.BlankAvatar; }
            set { AvatarJson = JsonConvert.SerializeObject(value); }
        }
        public virtual string GetAvatar()
        {
            if (AvatarJson.IsSet())
            {
                return Avatar.LargeUrl;
            }
            return MediaBase.BlankAvatar.LargeUrl;
        }
        #endregion

        #region Metadata
        public virtual string UserVars { get; set; }
        [NotMapped]
        public virtual Dictionary<string, string> Metadata
        {
            get
            {
                return UserVars.IsSet() ? JsonConvert.DeserializeObject<Dictionary<string, string>>(UserVars, new JsonSerializerSettings()
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new DefaultNamingStrategy()
                    }
                }) : new Dictionary<string, string>();
            }
            set
            {
                UserVars = JsonConvert.SerializeObject(value, new JsonSerializerSettings()
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new DefaultNamingStrategy()
                    }
                });
            }
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
    }
}
