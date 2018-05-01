using Hood.BaseTypes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    [Serializable]
    public class AccountSettings : SaveableModel
    {
        [Display(Name = "Registration Open")]
        public bool EnableRegistration { get; set; }

        [Display(Name = "Registration Type")]
        public string RegistrationType { get; set; }

        [Display(Name = "Registration Code Expiry Time")]
        public int CodeExpiry { get; set; }


        [Display(Name = "Send a Welcome Email")]
        public bool EnableWelcome { get; set; }

        [Display(Name = "Welcome Email title")]
        public string WelcomeTitle { get; set; }

        [Display(Name = "Welcome Email subject")]
        public string WelcomeSubject { get; set; }

        [Display(Name = "Welcome Email message")]
        public string WelcomeMessage { get; set; }

        [Display(Name = "Notify Administrators")]
        public bool NotifyNewAccount { get; set; }
        public AccountSettings()
        {
            // Set Defaults
            EnableRegistration = true;
            RegistrationType = "default";
            CodeExpiry = 48;
            WelcomeSubject = "Your new account: {SITETITLE}.";
            WelcomeTitle = "Your new account.";
            WelcomeMessage = "Your account has been successfully created, and you can log in and use your account straight away.";
        }
    }
}
