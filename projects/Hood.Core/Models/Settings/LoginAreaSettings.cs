using Hood.Extensions;
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Hood.Models
{
    public class LoginAreaSettings
    {
        public LoginAreaSettings()
        {
            Logo = "https://cdn.jsdelivr.net/npm/hoodcms@5.0.0-rc2/images/hood-cms.png";
            Title = "Hood CMS";
        }

        [Display(Name = "Area Logo", Description = "Change the logo for your login areas.")]
        public string Logo { get; set; }

        [Display(Name = "Area Title", Description = "Change the default title for your login areas.")]
        public string Title { get; set; }

        [Display(Name = "Background Images", Description = "Change the background images for your login areas. Enter an image url on each line, these can be local or public URLs.")]
        public string BackgroundImages { get; set; }

    }
}