using System;

namespace Hood.Models
{
    public class ErrorModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public Exception Error { get; set; }
        public string OriginalUrl { get; set; }
        public int Code { get; set; }
        public string ErrorMessage { get; set; }
    }

}
