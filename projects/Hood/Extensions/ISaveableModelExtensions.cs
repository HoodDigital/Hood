using Hood.Enums;
using Hood.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Extensions
{
    public static class ISaveableModelExtensions
    {
        public static void AddEditorMessage(this ISaveableModel model, EditorMessage? message)
        {
            if (message.HasValue)
            {
                switch (message.Value)
                {
                    case EditorMessage.Succeeded:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Operation completed successfully.";
                        break;
                    case EditorMessage.NotFound:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "Could not find the item requested in the database.";
                        break;
                    case EditorMessage.Sent:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Message sent successfully.";
                        break;
                    case EditorMessage.ErrorSending:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "There was a problem sending the message.";
                        break;
                    case EditorMessage.Duplicated:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "The content has been duplicated, you are now viewing the copied item.";
                        break;
                    case EditorMessage.ErrorDuplicating:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "There was a problem duplicating the content.";
                        break;
                    case EditorMessage.Created:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Created successfully.";
                        break;
                    case EditorMessage.Deleted:
                        model.MessageType = AlertType.Warning;
                        model.SaveMessage = "Deleted successfully.";
                        break;
                    case EditorMessage.CannotDeleteAdmin:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "Administrators cannot be deleted. Please remove this account from the Admin or SuperUser roles before deleting.";
                        break;
                    case EditorMessage.ImageUpdated:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Image updated successfully.";
                        break;
                    case EditorMessage.MediaRemoved:
                        model.MessageType = AlertType.Warning;
                        model.SaveMessage = "Media removed successfully.";
                        break;
                    case EditorMessage.Saved:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Saved successfully.";
                        break;
                    case EditorMessage.Archived:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Archived successfully.";
                        break;
                    case EditorMessage.Published:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Published successfully.";
                        break;
                    case EditorMessage.Activated:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Activated successfully.";
                        break;
                    case EditorMessage.Deactivated:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Deactivated successfully.";
                        break;
                    case EditorMessage.HomepageSet:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Homepage updated.";
                        break;
                    case EditorMessage.Error:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "An error occurred. If this continues to occur please contact support.";
                        break;
                    case EditorMessage.KeyCreated:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "You have a new API access key, this can now be used to communicate with the site, and access protected links.";
                        break;
                    case EditorMessage.KeyRolled:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "You have reset the API access key, this can now be used to communicate with the site, and access protected links.";
                        break;
                }
            }
        }
        public static void AddForumMessage(this ISaveableModel model, ForumMessage? message)
        {
            if (message.HasValue)
            {
                switch (message.Value)
                {
                    case ForumMessage.PostReported:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "Thank you. The post has been reported to our moderators.";
                        break;
                    case ForumMessage.NoPostFound:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "Could not find the post.";
                        break;
                    case ForumMessage.NoTopicFound:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "Could not find the topic.";
                        break;
                    case ForumMessage.PostDeleted:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "The post has been deleted.";
                        break;
                    case ForumMessage.TopicDeleted:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "The topic has been deleted.";
                        break;
                    case ForumMessage.Succeeded:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Operation completed successfully.";
                        break;
                    case ForumMessage.NotFound:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "Could not find the item requested in the database.";
                        break;
                    case ForumMessage.Sent:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Message sent successfully.";
                        break;
                    case ForumMessage.ErrorSending:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "There was a problem sending the message.";
                        break;
                    case ForumMessage.Created:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Created successfully.";
                        break;
                    case ForumMessage.Deleted:
                        model.MessageType = AlertType.Warning;
                        model.SaveMessage = "Deleted successfully.";
                        break;
                    case ForumMessage.ImageUpdated:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Image updated successfully.";
                        break;
                    case ForumMessage.MediaRemoved:
                        model.MessageType = AlertType.Warning;
                        model.SaveMessage = "Media removed successfully.";
                        break;
                    case ForumMessage.Saved:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Saved successfully.";
                        break;
                    case ForumMessage.Archived:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Archived successfully.";
                        break;
                    case ForumMessage.Published:
                        model.MessageType = AlertType.Success;
                        model.SaveMessage = "Published successfully.";
                        break;
                    case ForumMessage.Error:
                        model.MessageType = AlertType.Danger;
                        model.SaveMessage = "An error occurred. If this continues to occur please contact support.";
                        break;
                }
            }
        }
        public static void AddBillingMessage(this ISaveableModel model, BillingMessage? message)
        {
            switch (message)
            {
                case BillingMessage.Error:
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "There was an error updating or changing your subscription.Please try again. An unspecified error occurred.";
                    break;
                case BillingMessage.SubscriptionCreated:
                    model.MessageType = AlertType.Success;
                    model.SaveMessage = "Your subscription has been created and is ready to use right away!";
                    break;
                case BillingMessage.SubscriptionExists:
                    model.MessageType = AlertType.Warning;
                    model.SaveMessage = "You are already actively subscribed to this subscription plan!";
                    break;
                case BillingMessage.SubscriptionReactivated:
                    model.MessageType = AlertType.Success;
                    model.SaveMessage = "Your subscription has been re-activated. Changes to your access will be active right away.";
                    break;
                case BillingMessage.SubscriptionUpdated:
                    model.MessageType = AlertType.Success;
                    model.SaveMessage = "Your subscription has been updated. Changes to your access will be active right away.";
                    break;
                case BillingMessage.SubscriptionCancelled:
                    model.MessageType = AlertType.Warning;
                    model.SaveMessage = "Your subscription has been cancelled. You will no longer be billed, however your access will continue until your current billing cycle ends.";
                    break;
                case BillingMessage.SubscriptionEnded:
                    model.MessageType = AlertType.Info;
                    model.SaveMessage = "Your subscription has been ended. Changes to your access will be active right away.";
                    break;
                case BillingMessage.NoCustomerObject:
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "There was an error updating or changing your subscription. Please try again. Could not create your Stripe customer reference.";
                    break;
                case BillingMessage.NoStripeId:
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "There was an error updating or changing your subscription. Please try again. There is no Stripe account linked to your account.";
                    break;
                case BillingMessage.NoSubscription:
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "There was an error updating or changing your subscription. Please try again.<br />There is no Stripe account linked to your account.";
                    break;
                case BillingMessage.NoPaymentSource:
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "You cannot access this without an active subscription, please upgrade your account to view this area.";
                    break;
                case BillingMessage.UpgradeRequired:
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "You cannot upgrade your account as you do not have a valid payment source, please enter a valid credit or debit card to create your subscription.";
                    break;
                case BillingMessage.InvalidToken:
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "You cannot access this with your current subscription, please upgrade your subscription to view this area.";
                    break;
                case BillingMessage.StripeError:
                    model.MessageType = AlertType.Danger;
                    model.SaveMessage = "There was an error updating or changing your subscription. Please try again.";
                    break;
            }

        }
    }
}
