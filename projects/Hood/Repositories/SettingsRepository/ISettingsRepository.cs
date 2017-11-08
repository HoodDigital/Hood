using System.Collections.Generic;
using Hood.Models;
using Microsoft.Extensions.Configuration;
using Hood.Infrastructure;
using Hood.Models.Api;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Hood.Services
{
    public interface ISettingsRepository : IConfiguration
    {
        List<string> LockoutAccessCodes { get; }

        List<Option> AllSettings();
        bool Delete(string key);
        T Get<T>(string key, bool nocache = false);
        bool Set<T>(string key, T newValue);
        BasicSettings GetBasicSettings(bool noCache = false);
        IntegrationSettings GetIntegrationSettings(bool noCache = false);
        ContactSettings GetContactSettings(bool noCache = false);
        ContentSettings GetContentSettings(bool noCache = false);
        PropertySettings GetPropertySettings(bool noCache = false);
        BillingSettings GetBillingSettings(bool noCache = false);
        AccountSettings GetAccountSettings(bool noCache = false);
        MediaSettings GetMediaSettings(bool noCache = false);
        MailSettings GetMailSettings(bool noCache = false);
        SeoSettings GetSeo(bool noCache = false);
        OperationResult StripeEnabled();
        OperationResult PayPalEnabled();
        OperationResult SubscriptionsEnabled();
        string GetSiteTitle();
        OperationResult BillingEnabled();
        OperationResult CartEnabled();
        OperationResult PropertyEnabled();
        string GetVersion();
        Task ProcessCaptchaOrThrowAsync(HttpRequest request);
        string ReplacePlaceholders(string adminNoficationSubject);

        ContentApi ToContentApi(Content content);
        PropertyListingApi ToPropertyListingApi(PropertyListing property);
        ApplicationUserApi ToApplicationUserApi(IHoodUser user);
    }

}