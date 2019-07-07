using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Hood.Interfaces;
using Hood.Extensions;
using System;

namespace Hood.Models
{
    public class Response
    {
        public Array Data { get; set; }
        public int Count { get; set; }
        public string Errors { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public string Url { get; set; }
        public Exception Exception { get; set; }
        public IMediaObject Image { get; set; }

        public Response(Array data, int count, string message = "Succeeded!")
        {
            Success = true;
            Data = data;
            Message = message;
            Count = count;
        }

        public Response(string errors, string message = "An error occurred!")
        {
            Success = false;
            Message = message;
            Errors = errors;
        }

        public Response(bool success, string message = "Succeeded!")
        {
            Success = success;
            Message = message;
            Errors = message;
        }
        public Response(bool success, IMediaObject media, string message = "Succeeded!")
        {
            Success = success;
            Message = message;
            Errors = message;
            Image = media;
        }

        public Response(IEnumerable<IdentityError> errors, string message = "An error occurred!")
        {
            Success = false;
            Message = message;
            Errors = "";
            foreach (IdentityError err in errors)
            {
                Errors += err.Description + Environment.NewLine;

            }
        }
        public Response(Exception ex, string message = "An error occurred!")
        {
            Errors = ex.Message;
            Message = message;
            Success = false;
            Exception = ex;
        }
    }
}
