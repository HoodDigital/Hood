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
            Logo = "/hood/images/hood-cms.png";
            Title = "Hood CMS";
            BackgroundImages = "";
            for (int i = 1; i <= 9; i++)
            {
                BackgroundImages += $"/hood/images/bg/{i}.jpg{Environment.NewLine}";
            }
            BackgroundImagesFadeSpeed = 1000;
            BackgroundImagesChangeDuration = 3000;
            RandomiseImages = true;
        }

        [Display(Name = "Area Logo", Description = "Change the logo for your login areas.")]
        public string Logo { get; set; }

        [Display(Name = "Area Title", Description = "Change the default title for your login areas.")]
        public string Title { get; set; }

        [Display(Name = "Background Images", Description = "Change the background images for your login areas. Enter an image url on each line, these can be local or public URLs.")]
        public string BackgroundImages { get; set; }
        [Display(Name = "Background Fade Speed", Description = "Change the speed for your image transitions.")]
        public int BackgroundImagesFadeSpeed { get; set; } = 1000;
        [Display(Name = "Background Change Duration", Description = "Change the time between image transitions.")]
        public int BackgroundImagesChangeDuration { get; set; } = 3000;
        [Display(Name = "Randomise Background Images?", Description = "Check this to shuffle the background images every time the pages load.")]
        public bool RandomiseImages { get; set; } = true;

        public IHtmlContent BackgroundImagesScript
        {
            get
            {
                IEnumerable<string> imageList = BackgroundImages.Trim().Split(Environment.NewLine).ToList();
                string images = "[";
                if (RandomiseImages)
                {
                    imageList = imageList.Shuffle();
                }

                foreach (string img in imageList)
                {
                    images += $"'{img.Trim()}',";
                }
                images.TrimEnd(',');
                images += "]";
                return new HtmlString($@"<script>$.backstretch({images},{{fade:{BackgroundImagesFadeSpeed},duration:{BackgroundImagesChangeDuration}}});</script>");
            }
        }

    }
}