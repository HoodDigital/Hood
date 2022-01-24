using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web.Helpers;
using Hood.Core;
using Hood.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Hood.ViewModels
{
    public class SpamPreventionModel : IValidatableObject
    {
        public SpamPreventionModel()
        { }

        public const string HoneypotFieldName = "full_enquiry";

        [FromForm(Name = HoneypotFieldName)]
        public string Honeypot { get; set; }

        public const string TimestampFieldName = "ts";

        [FromForm(Name = TimestampFieldName)]
        public string Timestamp { get; set; }

        public const string HashFieldName = "hsh";

        [FromForm(Name = HashFieldName)]
        public string Hash { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var failedValidationResult = new ValidationResult("An error occured");
            var failedResult = new[] { failedValidationResult };

            if (this.Honeypot.IsSet() || !this.Hash.IsSet() || !this.Timestamp.IsSet())
            {
                return failedResult;
            }

            if (!Crypto.VerifyHashedPassword(this.Hash, this.Timestamp))
            {
                return failedResult;
            }

            DateTime timestamp;
            if (!DateTime.TryParseExact(this.Timestamp, "ffffHHMMyytssddmm", null, DateTimeStyles.None, out timestamp))
            {
                return failedResult;
            }

            // Check timestamp is within the last 20 minutes
            if (DateTime.UtcNow.AddMinutes(-10) > timestamp)
            {
                return failedResult;
            }

            return new[] { ValidationResult.Success };
        }
    }
}