using Hood.Enums;
using Hood.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.BaseTypes
{
    public class SaveableModel : ISaveableModel
    {
        [JsonIgnore]
        [NotMapped]
        public string SaveMessage { get; set; }

        [JsonIgnore]
        [NotMapped]
        public AlertType MessageType { get; set; }

    }
}
