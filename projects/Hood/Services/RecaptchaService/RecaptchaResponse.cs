using System;
using Newtonsoft.Json;

namespace Hood.Services
{
    public class RecaptchaResponse
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "score")]
        public decimal Score { get; set; }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "challenge_ts")]
        public DateTime ChallengeTS { get; set; }

        [JsonProperty(PropertyName = "hostname")]
        public string HostName { get; set; }
    }
}

