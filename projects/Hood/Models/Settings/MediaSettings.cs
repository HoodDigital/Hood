using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    [Serializable]
    public class MediaSettings
    {

        [Display(Name = "Azure Storage Key")]
        public string AzureKey { get; set; }

        [Display(Name = "Container Name")]
        public string ContainerName { get; set; }

        public MediaSettings()
        {
            ContainerName = Guid.NewGuid().ToString();
        }
    }
}
