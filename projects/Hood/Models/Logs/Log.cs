using Hood.Entities;
using Hood.Extensions;
using Hood.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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
        /// </summary>
        public string Detail { get; set; }

        [NotMapped]
        public Exception Exception
        {
            get { try { return Detail.IsSet() ? JsonConvert.DeserializeObject<Exception>(Detail) : null; } catch (Exception) { return null; } }
            set { try { Detail = JsonConvert.SerializeObject(value); } catch (Exception ex) { Detail = $"Could not serialize the exception: {ex.Message}"; } }
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
