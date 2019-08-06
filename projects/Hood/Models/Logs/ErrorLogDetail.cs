using System.Collections.Generic;

namespace Hood.Models
{
    public class ErrorLogDetail
    {
        public Dictionary<string, string> Exception { get; set; }
        public Dictionary<string, string> InnerException { get; set; }
        public string ObjectJson { get; set; }

    }
}
