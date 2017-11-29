using Hood.Enums;
using Hood.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.BaseTypes
{
    public abstract class SaveableModel : ISaveableModel
    {
        [NotMapped]
        public string SaveMessage { get; set; }
        [NotMapped]
        public AlertType MessageType { get; set; }

        public void AddEditorMessage(EditorMessage? message)
        {
            if (message.HasValue)
            {
                switch (message.Value)
                {
                    case EditorMessage.Created:
                        MessageType = AlertType.Success;
                        SaveMessage = "Created successfully.";
                        break;
                    case EditorMessage.Deleted:
                        MessageType = AlertType.Warning;
                        SaveMessage = "Deleted successfully.";
                        break;
                    case EditorMessage.ImageUpdated:
                        MessageType = AlertType.Success;
                        SaveMessage = "Image updated successfully.";
                        break;
                    case EditorMessage.MediaRemoved:
                        MessageType = AlertType.Warning;
                        SaveMessage = "Media removed successfully.";
                        break;
                    case EditorMessage.Saved:
                        MessageType = AlertType.Success;
                        SaveMessage = "Saved successfully.";
                        break;
                    case EditorMessage.Archived:
                        MessageType = AlertType.Success;
                        SaveMessage = "Archived successfully.";
                        break;
                    case EditorMessage.Published:
                        MessageType = AlertType.Success;
                        SaveMessage = "Published successfully.";
                        break;
                    case EditorMessage.HomepageSet:
                        MessageType = AlertType.Success;
                        SaveMessage = "Homepage updated.";
                        break;
                    case EditorMessage.Error:
                        MessageType = AlertType.Danger;
                        SaveMessage = "An error occurred. If this continues to occur please contact support.";
                        break;
                }
            }
        }

    }
}
