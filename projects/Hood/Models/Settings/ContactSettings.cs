using Hood.BaseTypes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    [Serializable]
    public class ContactSettings : SaveableModel
    {
        [Display(Name = "Form Submission Email Adddress")]
        public string Email { get; set; }
        [Display(Name = "User Email Subject")]
        public string Subject { get; set; }
        [Display(Name = "User Email Title")]
        public string Title { get; set; }
        [Display(Name = "User Email Message")]
        public string Message { get; set; }
        [Display(Name = "Email Notification Subject")]
        public string AdminNoficationSubject { get; set; }
        [Display(Name = "Email Notification Title")]
        public string AdminNoficationTitle { get; set; }
        [Display(Name = "Email Notification Message")]
        public string AdminNoficationMessage { get; set; }
        [Display(Name = "Contact Form Title")]
        public string ContactFormTitle { get; set; }
        [Display(Name = "Contact Form Message")]
        public string ContactFormMessage { get; set; }
        [Display(Name = "Contact \"Thank You\" Page URL")]
        public string ThankYouUrl { get; set; }
        [Display(Name = "Contact \"Thank You\" Title")]
        public string ThankYouTitle { get; set; }
        [Display(Name = "Contact \"Thank You\" Message")]
        public string ThankYouMessage { get; set; }

        public ContactSettings()
        {
            Email = "info@hooddigital.com";
            ContactFormTitle = "Send us a message";
            ContactFormMessage = "Someone will be in touch very soon...";
            ThankYouTitle = "Enquiry Sent!";
            ThankYouMessage = "Thank you for contacting us! Your enquiry has been successfully sent, and we are currently digesting it. We will be in touch once we have had a read. Thanks!";
            Subject = "Your enquiry has been submitted to {SITETITLE}.";
            Title = "Enquiry on it's way!";
            Message = "Thank you for contacting us! Your enquiry has been successfully sent, and we are currently digesting it. We will be in touch once we have had a read. Thanks!";
            AdminNoficationSubject = "A new enquiry from {SITETITLE}.";
            AdminNoficationTitle = "Enquiry recieved";
            AdminNoficationMessage = "A new enquiry has been recieved from the {SITETITLE} website.";
        }
    }
}
