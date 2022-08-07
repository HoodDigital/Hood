using Newtonsoft.Json;

namespace Hood.Models
{
    public class AuthToken
    {
        [JsonProperty("access_token")]
        public string Token { get; set; }
        [JsonProperty("token_type")]
        public string Type { get; set; }
        public string ToAuthHeader() { return $"{Type} {Token}"; }
    }
}