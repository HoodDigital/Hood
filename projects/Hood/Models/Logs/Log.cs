using Hood.Entities;
using Hood.Extensions;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{

    public class Log : BaseEntity<long>
    {
        /// <summary>
        /// Date and time of the logged event.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Logged in UserId at time of log.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Detailed description, or Json of exception for Exception logs.
        /// </summary>)
        /// 
        public string Message
        {
            get
            {
                if (ErrorLogDetail != null)
                {
                    foreach (var entry in ErrorLogDetail.Exception)
                    {
                        if (entry.Key == "Message")
                            return entry.Value;
                    }
                }
                return null;
            }
        }

        public string Detail { get; set; }

        [NotMapped]
        public ErrorLogDetail ErrorLogDetail
        {
            get { try { return Detail.IsSet() ? JsonConvert.DeserializeObject<ErrorLogDetail>(Detail) : null; } catch (Exception) { return null; } }
        }

        /// <summary>
        /// Logged in UserId at time of log.
        /// </summary>
        public LogType Type { get; set; }

        /// <summary>
        /// Logged in UserId at time of log.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Logged in user at time of log.
        /// </summary>
        public ApplicationUser User { get; set; }
        /// <summary>
        /// Source of the log in code. nameof(Function).
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// If the log arose from an active HttpRequest, source url goes here.
        /// </summary>
        public string SourceUrl { get; set; }
    }
}
