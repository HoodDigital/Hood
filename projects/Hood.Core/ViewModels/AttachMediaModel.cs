using Hood.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Hood.ViewModels
{
    public class AttachMediaModel
    {
        [FromQuery(Name = "field")]
        public string FieldName { get; set; }

        [FromForm(Name = "mediaId")]
        public int? MediaId { get; set; }

        public void ValidateOrThrow()
        {
            if (!MediaId.HasValue)
            {
                throw new Exception("No media id provided.");
            }
            if (!FieldName.IsSet())
            {
                throw new Exception("No field name provided.");
            }
        }
    }
}