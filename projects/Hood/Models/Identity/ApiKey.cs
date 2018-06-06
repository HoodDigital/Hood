using Hood.Entities;
using Hood.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Models
{
    public class ApiKey : BaseEntity<string>
    {
        public string Name { get; set; }
        public string Key { get; set; }

        public bool Active { get; set; }
        public DateTime CreatedOn { get; set; }

        public AccessLevel AccessLevel { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public List<ApiEvent> Events { get; set; }

        public string ToEncodedApiKeyPair()
        {
            string json = JsonConvert.SerializeObject(new ApiKeyPair()
            {
                Key = Key,
                Id = Id
            });
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }
    }

    public class ApiKeyPair
    {
        public ApiKeyPair()
        {
        }

        public ApiKeyPair(string encodedBase64String)
        {
            try
            {
                byte[] byteArray = Convert.FromBase64String(encodedBase64String);
                string json = Encoding.UTF8.GetString(byteArray);
                ApiKeyPair decoded = JsonConvert.DeserializeObject<ApiKeyPair>(json);

                Id = decoded.Id;
                Key = decoded.Key;
            }
            catch 
            {
                Id = null;
                Key = null;
            }
        }

        public string ToBase64EncodedString()
        {
            string json = JsonConvert.SerializeObject(this);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public string Id { get; set; }
        public string Key { get; set; }
    }
}
