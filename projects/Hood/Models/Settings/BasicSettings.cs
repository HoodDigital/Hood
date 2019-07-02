using Hood.BaseTypes;
using Hood.Extensions;
using Hood.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class BasicSettings : SaveableModel
    {
        public BasicSettings()
        {
            Title = "New Website";
            Logo = "/hood/images/hood-cms-dark.svg";
            LogoLight = "/hood/images/hood-cms-white.svg";
            Owner = new Person();
            Address = new SiteAddress();
        }

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

        [Display(Name = "Homepage", Description = "Choose a homepage for the site, other than the default Homepage.")]
        public int? Homepage { get; set; }

        [Display(Name = "Enable Api Keys", Description = "Allow external access to site data via the use of Api Keys, users can be issued with keys to access resources via the Api. <span class=\"badge badge-warning\">Beta</span>")]
        public bool EnableApiKeys { get; set; }
        [Display(Name = "Enable Themes", Description = "Enable the use of themes, when enabled, you can choose the site theme from the Settings > Themes page")]
        public bool EnableThemes { get; set; }
        [Display(Name = "Enable Preload", Description = "Preload will use the CSS preload method, and defer main site styles to load once downloaded. This will only work with Preloadable themes.")]
        public bool EnablePreload { get; set; }

        [Display(Name = "Lockout Mode", Description = "Lock the site and force users to view the Lockout Mode Holding Page. The login page will be available. Administrators can log in to bypass the Lockout Mode. You can include a form on the lockout page to provide code based access to the other areas.")]
        public bool LockoutMode { get; set; }
        [Display(Name = "Lock Login Page", Description = "If you set this, user's will need to enter an access code to view the login pages as well as the rest of the site.")]
        public bool LockLoginPage { get; set; }
        [Display(Name = "Lockout Mode Access Codes", Description = "Enter codes for access, <strong>one code per line</strong>, you can share these codes to grant access to the site.")]
        public string LockoutModeTokens { get; set; }
        [Display(Name = "Lockout Mode Holding Page", Description = "This is the page which your users will be redirected to when navigating to any part of the site, while lockout mode is active.")]
        public int? LockoutModeHoldingPage { get; set; }

        #endregion


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
        public string OwnerMobile { get; set; }
        /// <summary>
        /// Please use <see cref="BasicSettings.Owner.JobTitle"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string JobTitle { get => Owner.JobTitle; set => Owner.JobTitle = value; }
        /// <summary>
        /// Please use <see cref="BasicSettings.Title"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string SiteTitle { get; set; }
        /// <summary>
        /// Please use <see cref="BasicSettings.Owner.ToFullName()"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete(null, true)]
        public string OwnerFullName { get; set; }
        /// <summary>
        /// No replacement for this. Do not use.
        /// </summary>
        [Obsolete(null, true)]
        public string Description { get; set; }
        /// <summary>
        /// No replacement for this. Do not use.
        /// </summary>
        [Obsolete(null, true)]
        public string EditorType { get; set; }

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
