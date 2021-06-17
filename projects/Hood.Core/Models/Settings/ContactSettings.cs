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
        [Display(Name = "User Email Subject", Description = "For the email message sent to the person who submits the form.")]
        public string Subject { get; set; }
        [Display(Name = "User Email Title", Description = "For the email message sent to the person who submits the form.")]
        public string Title { get; set; }
        [Display(Name = "User Email Message", Description = "For the email message sent to the person who submits the form, the data from the form will also be included.")]
        public string Message { get; set; }
        [Display(Name = "Email Notification Subject", Description = "For the message sent to people who are notified about this contact form.")]
        public string AdminNoficationSubject { get; set; }
        [Display(Name = "Email Notification Title", Description = "For the message sent to people who are notified about this contact form.")]
        public string AdminNoficationTitle { get; set; }
        [Display(Name = "Email Notification Message", Description = "For the message sent to people who are notified about this contact form, the data from the form will also be included.")]
        public string AdminNoficationMessage { get; set; }
        [Display(Name = "Contact Form Title", Description = "The title displayed above the form on the contact page, if using a standard contact form template.")]
        public string ContactFormTitle { get; set; }
        [Display(Name = "Contact Form Message", Description = "The message displayed above the form on the contact page, if using a standard contact form template.")]
        public string ContactFormMessage { get; set; }
        [Display(Name = "Contact 'Thank You' Page URL", Description = "Use this to forward the user to a contact 'Thank You' page after submitting an enquiry. To simply display the message below and not forward, leave this field blank.")]
        public string ThankYouUrl { get; set; }
        [Display(Name = "Contact 'Thank You' Title", Description = "A message block will appear after the form is submitted, enter the title for this message block here.")]
        public string ThankYouTitle { get; set; }
        [Display(Name = "Contact 'Thank You' Message", Description = "A message block will appear after the form is submitted, enter the content for this message block here.")]
        public string ThankYouMessage { get; set; }

        public ContactSettings()
        {
            Email = "info@hooddigital.com";
            ContactFormTitle = "Send us a message";
            ContactFormMessage = "Someone will be in touch very soon...";
            ThankYouTitle = "Enquiry Sent!";
            ThankYouMessage = "<p>Thank you for contacting us! Your enquiry has been successfully sent.</p>";
            Subject = "Your enquiry has been submitted to {{Site.Title}}.";
            Title = "Enquiry on it's way!";
            Message = "<p>Thank you for contacting us! Your enquiry has been successfully sent.</p>";
            AdminNoficationSubject = "A new enquiry from {{Site.Title}}.";
            AdminNoficationTitle = "Enquiry received";
            AdminNoficationMessage = "<p>A new enquiry has been received from the <strong>{{Site.Title}}</strong> website.</p>";
        }
    }
}
