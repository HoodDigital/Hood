using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    [Serializable]
    public class MediaSettings
    {
        [Display(Name = "Azure Storage Key")]
        public string AzureKey { get; set; }

        [Display(Name = "Hood Image API Url")]
        public string HoodApiUrl { get; set; }

        [Display(Name = "Hood Image API Key")]
        public string HoodApiKey { get; set; }

        [Display(Name = "Container Name")]
        public string ContainerName { get; set; }

        public MediaSettings()
        {
            HoodApiUrl = "http://api.hooddigital.com/v2/thumb";
            ContainerName = Guid.NewGuid().ToString();
        }
    }
}
