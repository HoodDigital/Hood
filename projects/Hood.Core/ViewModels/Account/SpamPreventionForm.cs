using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Hood.BaseTypes;
using Hood.Core;
using Hood.Extensions;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hood.ViewModels
{
    public class SpamPreventionModel : SaveableModel, IValidatableObject
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
        public const string SaltFieldName = "slt";

        [FromForm(Name = SaltFieldName)]
        public string Salt { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var failedValidationResult = new ValidationResult("An error occured");
            var failedResult = new[] { failedValidationResult };

            if (this.Honeypot.IsSet() || !this.Hash.IsSet() || !this.Timestamp.IsSet())
            {
                return failedResult;
            }

            var hasher = new PasswordHasher(this.Salt);
            hasher = hasher.HashPasswordWithSalt(this.Timestamp);
            var hash = hasher.HashedPassword;
            var salt = hasher.Base64Salt;

            if (hash != this.Hash)
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