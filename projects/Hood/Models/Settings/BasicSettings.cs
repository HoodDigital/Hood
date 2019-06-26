using Hood.BaseTypes;
using Hood.Extensions;
using Hood.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class BasicSettings : SaveableModel
    {
        public Person Owner { get; set; }
        public SiteAddress Address { get; set; }

        #region Site Settings

        [Display(Name = "Site Title")]
        public string Title { get; set; }
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Site Logo")]
        public string Logo { get; set; }
        [Display(Name = "Site Logo (Light)")]
        public string LogoLight { get; set; }

        [Display(Name = "Site/Company Phone Number")]
        public string Phone { get; set; }
        [Display(Name = "Site/Company Email")]
        public string Email { get; set; }

        [Display(Name = "Homepage")]
        public int? Homepage { get; set; }

        [Display(Name = "Enable Api Keys")]
        public bool EnableApiKeys { get; set; }
        [Display(Name = "Enable Themes")]
        public bool EnableThemes { get; set; }
        [Display(Name = "Enable Preload")]
        public bool EnablePreload { get; set; }

        [Display(Name = "Lockout Mode")]
        public bool LockoutMode { get; set; }
        [Display(Name = "Lock Login Page")]
        public bool LockLoginPage { get; set; }
        [Display(Name = "Lockout Mode Access Codes")]
        public string LockoutModeTokens { get; set; }
        [Display(Name = "Lockout Mode Holding Page")]
        public int? LockoutModeHoldingPage { get; set; }

        #endregion

        public BasicSettings()
        {
            Title = "New Website";
            Logo = "/hood/images/hood-cms-dark.svg";
            LogoLight = "/hood/images/hood-cms-white.svg";
        }

        public string FullTitle
        {
            get
            {
                if (Title.IsSet())
                    return Title;
                if (CompanyName.IsSet())
                    return CompanyName;
                return "Untitled Site";
            }
        }

        #region Obsolete
        /// <summary>
        /// Please use <see cref="BasicSettings.Logo"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string SiteLogo { get => Logo; set => Logo = value; }
        /// <summary>
        /// Please use <see cref="BasicSettings.LogoLight"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string SiteLogoLight { get => LogoLight; set => LogoLight = value; }
        /// <summary>
        /// Please use <see cref="BasicSettings.Owner.FirstName"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string OwnerFirstName { get => Owner.FirstName; set => Owner.FirstName = value; }
        /// <summary>
        /// Please use <see cref="BasicSettings.Owner.LastName"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string OwnerLastName { get => Owner.LastName; set => Owner.LastName = value; }
        /// <summary>
        /// Please use <see cref="BasicSettings.Owner.ToDisplayName()"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string OwnerDisplayName { get => Owner.DisplayName; set => Owner.DisplayName = value; }
        /// <summary>
        /// Please use <see cref="BasicSettings.Owner.Phone"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string OwnerPhone { get => Owner.Phone; set => Owner.Phone = value; }
        /// <summary>
        /// No replacement for this. Do not use.
        /// </summary>
        [Obsolete(null, true)]
        public string OwnerMobile => throw new NotImplementedException();
        /// <summary>
        /// Please use <see cref="BasicSettings.Owner.JobTitle"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string JobTitle { get => Owner.JobTitle; set => Owner.JobTitle = value; }
        /// <summary>
        /// Please use <see cref="BasicSettings.Title"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string SiteTitle { get => Title; set => Title = value; }
        /// <summary>
        /// Please use <see cref="BasicSettings.Owner.ToFullName()"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string OwnerFullName { get => Owner.ToFullName(); set => throw new NotImplementedException(); }
        /// <summary>
        /// No replacement for this. Do not use.
        /// </summary>
        [Obsolete(null, true)]
        public string Description => throw new NotImplementedException();
        /// <summary>
        /// No replacement for this. Do not use.
        /// </summary>
        [Obsolete(null, true)]
        public string EditorType => throw new NotImplementedException();

        #endregion
    }

    public partial class SiteAddress : IAddress
    {

        public string ContactName { get; set; }

        [Display(Name = "Building Name/Number")]
        public string Number { get; set; }
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
