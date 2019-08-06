using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Hood.Interfaces;
using System;
using Hood.Extensions;

namespace Hood.Models
{
    public class Response
    {
        public Array Data { get; set; }
        public int Count { get; set; }
        public string Errors { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }
        public bool Success { get; set; }
        public string Url { get; set; }
        public Dictionary<string, string> Exception { get; set; }
        public IMediaObject Media { get; set; }
        public string MediaJson { get { return Media.ToJson(); } }

        public Response(Array data, int count, string message = "", string title = "Succeeded!")
        {
            Success = true;
            Data = data;
            Message = message;
            Title = title;
            Count = count;
        }

        public Response(string errors, string message = "", string title = "An error occurred!")
        {
            Success = false;
            Message = message;
            Title = title;
            Errors = errors;
        }

        public Response(bool success, string message = "", string title = null)
        {
            Success = success;
            Message = message;
            Title = title.IsSet() ? title : success ? "Succeeded" : "Failed";
            Errors = message;
        }
        public Response(bool success, IMediaObject media, string message = "", string title = null)
        {
            Success = success;
            Message = message;
            Errors = message;
            Title = title.IsSet() ? title : success ? "Succeeded" : "Failed";
            Media = media;
        }

        public Response(IEnumerable<IdentityError> errors, string message = "", string title = "An error occurred!")
        {
            Success = false;
            Message = message;
            Title = title;
            Errors = "";
            foreach (IdentityError err in errors)
            {
                Errors += err.Description + "<br />";

            }
        }
        public Response(IEnumerable<string> errors, string message = "", string title = "An error occurred!")
        {
            Success = false;
            Message = message;
            Title = title;
            Errors = "";
            foreach (string err in errors)
            {
                Errors += err + "<br />";

            }
        }
        public Response(Exception ex, string message = "", string title = "An error occurred!")
        {
            Success = false;
            Errors = ex.Message;
            Message = message;
            Title = title;
            Exception = ex.ToDictionary();
        }
    }
}
