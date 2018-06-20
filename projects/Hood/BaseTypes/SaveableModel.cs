using Hood.Enums;
using Hood.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.BaseTypes
{
    public class SaveableModel : ISaveableModel
    {
        [NotMapped]
        public string SaveMessage { get; set; }
        [NotMapped]
        public AlertType MessageType { get; set; }
        [NotMapped]
        public int? MessageId { get; set; }
    }
}
