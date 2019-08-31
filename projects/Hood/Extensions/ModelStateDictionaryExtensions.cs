using Hood.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Hood.Extensions
{
    public static class ModelStateDictionaryExtensions
    {
        public static bool IsNotSpam<TModel>(this ModelStateDictionary modelState, TModel model)
            where TModel : HoneyPotFormModel
        {
            if (model.IsSpambot)
            {
                modelState.AddModelError(string.Empty, "You have been flagged as a spammer. If this is not the case, please contact us directly.");
                return false;
            }
            return true;
        }

    }
}
