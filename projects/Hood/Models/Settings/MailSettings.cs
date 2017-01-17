using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    [Serializable]
    public class MailSettings
    {
        [NonSerialized]
        public const string PlainTemplate = "Areas/Admin/Views/Mail/Plain.cshtml";
        [NonSerialized]
        public const string SuccessTemplate = "Areas/Admin/Views/Mail/Success.cshtml";
        [NonSerialized]
        public const string WarningTemplate = "Areas/Admin/Views/Mail/Warning.cshtml";
        [NonSerialized]
        public const string DangerTemplate = "Areas/Admin/Views/Mail/Danger.cshtml";

        [Display(Name = "SendGrid Api Key")]
        public string SendGridKey { get; set; }

        [Display(Name = "From Name")]
        public string FromName { get; set; }

        [Display(Name = "From Email")]
        public string FromEmail { get; set; }

        #region "Design"
        [Display(Name = "Background Colour")]
        public string BackgroundColour { get; set; }
        [Display(Name = "Logo")]
        public string Logo { get; set; }
        #endregion

        public MailSettings()
        {
            BackgroundColour = "#f6f6f6";
        }
    }
}
