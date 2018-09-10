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

        [Display(Name = "Azure Storage Host (Or CDN Endpoint Host)")]
        public string AzureHost { get; set; }

        [Display(Name = "Azure Storage Scheme")]
        public string AzureScheme { get; set; }

        [Display(Name = "Container Name")]
        public string ContainerName { get; set; }

        [Display(Name = "[No Image] File")]
        public string NoImage { get; set; }

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
