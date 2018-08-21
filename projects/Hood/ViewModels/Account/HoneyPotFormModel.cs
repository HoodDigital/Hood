using Hood.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace Hood.ViewModels
{
    public class HoneyPotFormModel
    {
        [FromForm(Name = "zip_code")]
        public string Honeypot { get; set; }

        public bool IsSpambot
        {
            get
            {
                if (Honeypot.IsSet())
                    return true;
                return false;
            }
        }

    }
}