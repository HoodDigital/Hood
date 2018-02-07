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
                    case EditorMessage.Succeeded:
                        MessageType = AlertType.Success;
                        SaveMessage = "Operation completed successfully.";
                        break;
                    case EditorMessage.NotFound:
                        MessageType = AlertType.Danger;
                        SaveMessage = "Could not find the item requested in the database.";
                        break;
                    case EditorMessage.Sent:
                        MessageType = AlertType.Success;
                        SaveMessage = "Message sent successfully.";
                        break;
                    case EditorMessage.ErrorSending:
                        MessageType = AlertType.Danger;
                        SaveMessage = "There was a problem sending the message.";
                        break;
                    case EditorMessage.Duplicated:
                        MessageType = AlertType.Success;
                        SaveMessage = "The content has been duplicated, you are now viewing the copied item.";
                        break;
                    case EditorMessage.ErrorDuplicating:
                        MessageType = AlertType.Danger;
                        SaveMessage = "There was a problem duplicating the content.";
                        break;
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

        public void AddBillingMessage(BillingMessage? message)
        {
            switch (message)
            {
                case BillingMessage.Error:
                    MessageType = AlertType.Danger;
                    SaveMessage = "There was an error updating or changing your subscription.Please try again. An unspecified error occurred.";
                    break;
                case BillingMessage.SubscriptionCreated:
                    MessageType = AlertType.Success;
                    SaveMessage = "Your subscription has been created and is ready to use right away!";
                    break;
                case BillingMessage.SubscriptionExists:
                    MessageType = AlertType.Warning;
                    SaveMessage = "You are already actively subscribed to this subscription plan!";
                    break;
                case BillingMessage.SubscriptionReactivated:
                    MessageType = AlertType.Success;
                    SaveMessage = "Your subscription has been re-activated. Changes to your access will be active right away.";
                    break;
                case BillingMessage.SubscriptionUpdated:
                    MessageType = AlertType.Success;
                    SaveMessage = "Your subscription has been updated. Changes to your access will be active right away.";
                    break;
                case BillingMessage.SubscriptionCancelled:
                    MessageType = AlertType.Warning;
                    SaveMessage = "Your subscription has been cancelled. You will no longer be billed, however your access will continue until your current billing cycle ends.";
                    break;
                case BillingMessage.SubscriptionEnded:
                    MessageType = AlertType.Info;
                    SaveMessage = "Your subscription has been ended. Changes to your access will be active right away.";
                    break;
                case BillingMessage.NoCustomerObject:
                    MessageType = AlertType.Danger;
                    SaveMessage = "There was an error updating or changing your subscription. Please try again. Could not create your Stripe customer reference.";
                    break;
                case BillingMessage.NoStripeId:
                    MessageType = AlertType.Danger;
                    SaveMessage = "There was an error updating or changing your subscription. Please try again. There is no Stripe account linked to your account.";
                    break;
                case BillingMessage.NoSubscription:
                    MessageType = AlertType.Danger;
                    SaveMessage = "There was an error updating or changing your subscription. Please try again.<br />There is no Stripe account linked to your account.";
                    break;
                case BillingMessage.NoPaymentSource:
                    MessageType = AlertType.Danger;
                    SaveMessage = "You cannot access this without an active subscription, please upgrade your account to view this area.";
                    break;
                case BillingMessage.UpgradeRequired:
                    MessageType = AlertType.Danger;
                    SaveMessage = "You cannot upgrade your account as you do not have a valid payment source, please enter a valid credit or debit card to create your subscription.";
                    break;
                case BillingMessage.InvalidToken:
                    MessageType = AlertType.Danger;
                    SaveMessage = "You cannot access this with your current subscription, please upgrade your subscription to view this area.";
                    break;
                case BillingMessage.StripeError:
                    MessageType = AlertType.Danger;
                    SaveMessage = "There was an error updating or changing your subscription. Please try again.";
                    break;
            }

        }

    }
}
