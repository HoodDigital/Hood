using Hood.Enums;
using Hood.Interfaces;

namespace Hood.Extensions
{
    public static class ISaveableModelExtensions
    {
        public static void Test(this ISaveableModel model)
        {
#if false
            await _logService.AddExceptionAsync<ApiController>(SaveMessage, ex);

            try
            {
                SaveMessage = $"";
                MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = $"An error occurred while : {ex.Message}";
                MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<ApiController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Index));


            SaveMessage = $"An error occurred while : {ex.Message}";
            await _logService.AddExceptionAsync<PropertyController>(SaveMessage, ex);
            return new Response(SaveMessage);

            throw new Exception("");

#warning TODO: Handle response in JS.

#endif
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
