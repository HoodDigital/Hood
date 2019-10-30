using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class AdminAreaSettings
    {
        public AdminAreaSettings()
        {
            Logo = "/hood/images/hood-cms.png";
            Title = "Hood CMS";
        }

        [Display(Name = "Admin Area Logo", Description = "Add a custom logo to your admin areas.")]
        public string Logo { get; set; }

        [Display(Name = "Admin Area Title", Description = "Change the default title for your admin areas.")]
        public string Title { get; set; }
    }
}