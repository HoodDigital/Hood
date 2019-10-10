using Hood.Entities;
using Hood.Enums;
using Hood.Extensions;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{

    public class ApiEvent : BaseEntity<long>
    {
        public string ApiKeyId { get; set; }
        public ApiKey ApiKey { get; set; }

        public DateTime Time { get; set; }
        public string IpAddress { get; set; }

        public string Url { get; set; }
        public string Action { get; set; }
        public string RouteDataJson { get; set; }

        public AccessLevel RequiredAccessLevel { get; set; }

        [NotMapped]
        public RouteValueDictionary RouteData
        {
            get { return RouteDataJson.IsSet() ? JsonConvert.DeserializeObject<RouteValueDictionary>(RouteDataJson) : null; }
            set { RouteDataJson = JsonConvert.SerializeObject(value); }
        }
    }
}