using System;

namespace Hood.Services
{
    public class RecaptchaResponse
    {
        public RecaptchaResponse() { }

        public RecaptchaResponse(bool success, string message)
        {
            Passed = success;
            Message = message;
        }

        internal bool Success { get; set; }

        internal decimal Score { get; set; }

        internal string Action { get; set; }

        internal DateTime ChallengeTS { get; set; }

        internal string HostName { get; set; }

        public bool Passed { get; internal set; }

        public string Message { get; internal set; }
    }
}
