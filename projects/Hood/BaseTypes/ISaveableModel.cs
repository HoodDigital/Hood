using Hood.Enums;

namespace Hood.Interfaces
{
    public interface ISaveableModel
    {
        AlertType MessageType { get; set; }
        string SaveMessage { get; set; }
        int? MessageId { get; set; }
    }
}