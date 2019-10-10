using Hood.Core;
using Hood.Interfaces;
using Hood.Models;

namespace Hood.Extensions
{
    public static class StringPlaceholderExtensions
    {
        public static string ReplaceSiteVariables(this string text)
        {
            if (!text.IsSet())
                return text;

            var settings = Engine.Settings.Basic;
            return text
                .Replace("{Site.Title}", settings.FullTitle)
                .Replace("{SITETITLE}", settings.FullTitle) // Backwards Compat Removed-v3.0.0
                .Replace("{Site.CompanyName}", settings.CompanyName)
                .Replace("{Site.Phone}", settings.Phone)
                .Replace("{Site.Logo}", settings.Logo)
                .Replace("{Site.LogoLight}", settings.LogoLight)
                .Replace("{Site.Address}", settings.Address.ToFormat(Enums.AddressFormat.SingleLine))
                .Replace("{Site.Owner.Name}", settings.Owner.ToFullName())
                .Replace("{Site.Owner.Phone}", settings.Owner.Phone)
                .Replace("{Site.Owner.Email}", settings.Owner.Email);
        }
        public static string ReplaceUserVariables(this string text, IUserProfile user)
        {
            if (!text.IsSet() || user == null)
                return text;

            return text
                .Replace("{User.Username}", user.UserName)
                .Replace("{User.Email}", user.Email)
                .Replace("{User.PhoneNumber}", user.PhoneNumber)
                .Replace("{User.Facebook}", user.Facebook)
                .Replace("{User.LinkedIn}", user.LinkedIn)
                .Replace("{User.Twitter}", user.Twitter)
                .Replace("{User.Instagram}", user.Instagram)
                .Replace("{User.WebsiteUrl}", user.WebsiteUrl)
                .Replace("{User.FullName}", user.ToFullName())
                .Replace("{FULLNAME}", user.ToFullName())
                .Replace("{User.FirstName}", user.FirstName)
                .Replace("{User.LastName}", user.LastName);
        }
    }
}
