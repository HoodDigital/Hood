using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Hood.Interfaces;
using Hood.Extensions;
using System;

namespace Hood.Models
{
    [Obsolete("Use Hood.Models.Response from now on.", false)]
    public class MediaResponse : Response
    {
        public MediaResponse(Array data, int count, string message = "Succeeded!")
            : base(data, count, message)
        { }
        public MediaResponse(string errors)
            : base(errors)
        { }
        public MediaResponse(bool success, string message = "Succeeded!")
            : base(success, message)
        { }
        public MediaResponse(bool success, IMediaObject media)
            : base(success)
        {
            Media = media;
            Json = media.ToJson();
        }
        public MediaResponse(Exception ex)
               : base(ex)
        { }

        public IMediaObject Media { get; set; }
        public string Json { get; set; }
    }

    public class Response
    {
        public Array Data { get; set; }
        public int Count { get; set; }
        public string Errors { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public string Url { get; set; }
        public Exception Exception { get; set; }

        public Response(Array data, int count, string message = "Succeeded!")
        {
            Success = true;
            Data = data;
            Message = message;
            Count = count;
        }

        public Response(string errors)
        {
            Success = false;
            Errors = errors;
        }

        public Response(bool success, string message = "Succeeded!")
        {
            Success = success;
            Message = message;
        }

        public Response(IEnumerable<IdentityError> errors)
        {
            Success = false;
            Errors = "";
            foreach (IdentityError err in errors)
            {
                Errors += err.Description + Environment.NewLine;

            }
        }
        public Response(Exception ex)
        {
            Errors = ex.Message;
            Success = false;
            Exception = ex;
        }
    }

}
