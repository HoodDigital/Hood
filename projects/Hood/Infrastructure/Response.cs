using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Hood.Models
{
    public class Response
    {
        public Array Data { get; set; }
        public int Count { get; set; }
        public string Errors { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
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
            this.Exception = ex;
        }
    }

}
