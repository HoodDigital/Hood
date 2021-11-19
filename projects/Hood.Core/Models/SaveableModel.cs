using Hood.Attributes;
using Hood.Enums;
using Hood.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

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
