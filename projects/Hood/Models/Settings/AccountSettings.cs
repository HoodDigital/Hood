using Hood.BaseTypes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public enum RegistrationType
    {
        Default,
        Code
    }

    [Serializable]
    public class AccountSettings : SaveableModel
    {
        [Display(Name = "Registration Open", Description = "Disabling registration will prevent users signing up to your site, you will have to add users manually.")]
        public bool EnableRegistration { get; set; }

        [Display(Name = "Registration Type", Description = "Email Code based registration will force the user to validate their email address before completing registration.")]
        public RegistrationType RegistrationType { get; set; }

        [Display(Name = "Registration Code Expiry Time", Description = "Set (in hours) the time for your registration codes to expire, user's will need to resend their code if they wait too long.")]
        public int CodeExpiry { get; set; }


        [Display(Name = "Send a Welcome Email", Description = "This will send a formatted welcome email to new account holders.")]
        public bool EnableWelcome { get; set; }
        [Display(Name = "Require Email Confirmation", Description = "This will send confirmation email to new account holders, they will not be able to log in until their email is confirmed.")]
        public bool RequireEmailConfirmation { get; set; }

        [Display(Name = "Welcome Email title")]
        public string WelcomeTitle { get; set; }

        [Display(Name = "Welcome Email subject")]
        public string WelcomeSubject { get; set; }

        [Display(Name = "Welcome Email message")]
        public string WelcomeMessage { get; set; }

        [Display(Name = "Verify Email title")]
        public string VerifyTitle { get; set; }

        [Display(Name = "Verify Email subject")]
        public string VerifySubject { get; set; }

        [Display(Name = "Verify Email message")]
        public string VerifyMessage { get; set; }

        [Display(Name = "Notify Administrators", Description = "This will copy in the NewAccountNotifications role on the above email.")]
        public bool NotifyNewAccount { get; set; }
        public AccountSettings()
        {
            // Set Defaults
            EnableRegistration = true;
            RegistrationType = RegistrationType.Default;
            CodeExpiry = 48;
            WelcomeSubject = "Your new account: {Site.Title}.";
            WelcomeTitle = "Your new account.";
            WelcomeMessage = "Your account has been successfully created, and you can log in and use your account straight away.";
        }
    }
}
