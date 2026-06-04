using System.ComponentModel.DataAnnotations.Schema;
using Hood.Attributes;
using Hood.Enums;
using Hood.Interfaces;
using Newtonsoft.Json;

namespace Hood.BaseTypes
{
    public class SaveableModel : ISaveableModel
    {
        [JsonIgnore]
        [RouteIgnore]
        [NotMapped]
        public string SaveMessage { get; set; }

        [JsonIgnore]
        [RouteIgnore]
        [NotMapped]
        public AlertType MessageType { get; set; }
    }
}
