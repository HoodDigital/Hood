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
        public DateTime Time { get; set; }

        public string Title { get; set; }
        public string Detail { get; set; }

        public LogType Type { get; set; }
        public LogSource Source { get; set; }

        public string ExceptionJson { get; set; }
        [NotMapped]
        public Exception Exception
        {
            get { return ExceptionJson.IsSet() ? JsonConvert.DeserializeObject<Exception>(ExceptionJson) : null; }
            set { ExceptionJson = JsonConvert.SerializeObject(value); }
        }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string EntityId { get; set; }
        public string EntityType { get; set; }

        public string SourceUrl { get; set; }
    }
}
