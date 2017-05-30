using Hood.BaseTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hood.Models
{
    [Serializable]
    public class AccountSettings : SaveableModel
    {
        [Display(Name = "Registration Open")]
        public bool EnableRegistration { get; set; }

        [Display(Name = "Registration Type")]
        public string RegistrationType { get; set; }

        [Display(Name = "Registration Type")]
        public int CodeExpiry { get; set; }

        public AccountSettings()
        {
            // Set Defaults
            EnableRegistration = true;
            RegistrationType = "default";
            CodeExpiry = 48;
        }
    }
}
