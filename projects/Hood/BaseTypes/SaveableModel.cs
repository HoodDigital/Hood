using Hood.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.BaseTypes
{
    public abstract class SaveableModel
    {
        [NotMapped]
        public string SaveMessage { get; set; }
        [NotMapped]
        public AlertType MessageType { get; set; }
    }
}
