using Hood.BaseTypes;
using Hood.Services;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    [Serializable]
    public class MediaSettings : SaveableModel
    {
        [Display(Name = "Azure Storage Key")]
        public string AzureKey { get; set; }

        [Display(Name = "Azure Storage Host (Or CDN Endpoint Host)", Description = "Images or files may take a few minutes to propogate and cache on your CDN endpoint, if you chose to use one.")]
        public string AzureHost { get; set; }

        [Display(Name = "Azure Storage Scheme")]
        public string AzureScheme { get; set; }

        [Display(Name = "Container Name", Description = "This is the container (folder) name that the site will use to store media uploads on your Azure Storage account.")]
        public string ContainerName { get; set; }

        [Display(Name = "No Image File", Description = "This will show in place of uploaded images for content/items etc. when no image is set.")]
        public string NoImage { get; set; }
        [Display(Name = "No Profile Image File", Description = "This will show in place of uploaded images for user avatars when no image is set.")]
        public string NoProfileImage { get; set; }

        [NonSerialized]
        private MediaRefreshReport _UpdateReport;
        public MediaRefreshReport UpdateReport { get { return _UpdateReport; } set { _UpdateReport = value; } }

        public MediaSettings()
        {
            ContainerName = Guid.NewGuid().ToString();
            AzureScheme = "https";
        }
    }
}
