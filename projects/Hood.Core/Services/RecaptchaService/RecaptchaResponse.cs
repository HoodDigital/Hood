using System;
using Newtonsoft.Json;

namespace Hood.Services
{
    public class RecaptchaResponse
    {
        public RecaptchaResponse()
        { }

        public RecaptchaResponse(bool success, string message)
        {
            Passed = success;
            Message = message;
        }        

        [JsonProperty(PropertyName = "success")]
        internal bool Success { get; set; }

        [JsonProperty(PropertyName = "score")]
        internal decimal Score { get; set; }

        [JsonProperty(PropertyName = "action")]
        internal string Action { get; set; }

        [JsonProperty(PropertyName = "challenge_ts")]
        internal DateTime ChallengeTS { get; set; }

        [JsonProperty(PropertyName = "hostname")]
        internal string HostName { get; set; }

        [JsonIgnore]
        public bool Passed { get; internal set; }

        [JsonIgnore]
        public string Message { get; internal set; }

    }
}

