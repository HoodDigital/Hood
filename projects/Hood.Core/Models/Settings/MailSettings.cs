﻿using Hood.BaseTypes;
using Hood.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    [Serializable]
    public class MailSettings : SaveableModel
    {
        [NonSerialized]
        public const string PlainTemplate = "Areas/Admin/UI/Mail/Plain.cshtml";
        [NonSerialized]
        public const string SuccessTemplate = "Areas/Admin/UI/Mail/Success.cshtml";
        [NonSerialized]
        public const string WarningTemplate = "Areas/Admin/UI/Mail/Warning.cshtml";
        [NonSerialized]
        public const string DangerTemplate = "Areas/Admin/UI/Mail/Danger.cshtml";

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
        [Display(Name = "Hero Image")]
        public string HeroImage { get; set; }
        public bool IsSetup
        {
            get
            {
                return SendGridKey.IsSet() && FromEmail.IsSet();
            }
        }
        #endregion

        public MailSettings()
        {
            BackgroundColour = "#f6f6f6";
            FromEmail = "yourwebsite@cms.hooddigital.com";
            FromName = "Hood CMS Website";
        }
    }
}
