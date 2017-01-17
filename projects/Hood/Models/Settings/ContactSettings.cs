using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    [Serializable]
    public class ContactSettings
    {
        [Display(Name = "Contact Page Title")]
        public string Title { get; set; }
        [Display(Name = "Contact Form Email")]
        public string Email { get; set; }
        [Display(Name = "Business Hours")]
        public string BusinessHours { get; set; }
        [Display(Name = "Contact Page Message")]
        public string Message { get; set; }
        [Display(Name = "Contact \"Thank You\" Page URL")]
        public string ThankYouUrl { get; set; }
        [Display(Name = "Contact \"Thank You\" Message")]
        public string ThankYouMessage { get; set; }

        public ContactSettings()
        {
            Email = "info@hooddigital.com";
            ThankYouMessage = "Thank you for contacting us! Your enquiry has been successfully sent, and we are currently digesting it. We will be in touch once we have had a read. Thanks!";
        }
    }
}
