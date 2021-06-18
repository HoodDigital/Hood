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
            Logo = "https://cdn.jsdelivr.net/npm/hoodcms@5.0.0-rc1/images/hood-cms.png";
            LogoLight = "https://cdn.jsdelivr.net/npm/hoodcms@5.0.0-rc1/images/hood-cms-white.png";
            Owner = new Person();
            Address = new SiteAddress();
            LoginAreaSettings = new LoginAreaSettings();
            AdminAreaSettings = new AdminAreaSettings();
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

        [Display(Name = "Enable Themes", Description = "Enable the use of themes, when enabled, you can choose the site theme from the Settings > Themes page")]
        public bool EnableThemes { get; set; }


        #endregion

        public LoginAreaSettings LoginAreaSettings { get; set; }
        public AdminAreaSettings AdminAreaSettings { get; set; }


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
