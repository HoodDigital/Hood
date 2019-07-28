using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hood.Infrastructure;
using Hood.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Hood.Services
{
    public interface ISettingsRepository : IConfiguration
    {

        #region Get/Set/Delete
        T Get<T>(string key = null);
        void Set<T>(T value, string key = null);
        void Remove<T>(string key = null);
        #endregion

        #region Site Settings
        AccountSettings Account { get; }
        BasicSettings Basic { get; }
        BillingSettings Billing { get; }
        ContactSettings Contact { get; }
        ContentSettings Content { get; }
        ForumSettings Forum { get; }
        IntegrationSettings Integrations { get; }
        MailSettings Mail { get; }
        MediaSettings Media { get; }
        PropertySettings Property { get; }
        SeoSettings Seo { get; }
        UserProfile SiteOwner { get; }
        #endregion

        #region Other Settings/Properties
        string ConnectionString { get; }
        List<string> LockoutAccessCodes { get; }

        #endregion

        #region Obsoletes
        /// <summary>
        /// Please use <see cref="ISettingsRepository.Basic"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Basic instead.", true)]
        BasicSettings GetBasicSettings(bool noCache = false);

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Integrations"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Integrations instead.", true)]
        IntegrationSettings GetIntegrationSettings(bool noCache = false);

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Contact"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Contact instead.", true)]
        ContactSettings GetContactSettings(bool noCache = false);

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Seo"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Seo instead.", true)]
        SeoSettings GetSeo(bool noCache = false);

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Content"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Content instead.", true)]
        ContentSettings GetContentSettings(bool noCache = false);

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Property"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Property instead.", true)]
        PropertySettings GetPropertySettings(bool noCache = false);

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Billing"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Billing instead.", true)]
        BillingSettings GetBillingSettings(bool noCache = false);

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Account"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Account instead.", true)]
        AccountSettings GetAccountSettings(bool noCache = false);

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Basic"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Media instead.", true)]
        MediaSettings GetMediaSettings(bool noCache = false);

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Mail"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Mail instead.", true)]
        MailSettings GetMailSettings(bool noCache = false);

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Forum"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Forum instead.", true)]
        ForumSettings GetForumSettings(bool noCache = false);

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Billing.StripeEnabled"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Billing.StripeEnabled instead.", true)]
        OperationResult StripeEnabled();

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Billing.PayPalEnabled"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Billing.PayPalEnabled instead.", true)]
        OperationResult PayPalEnabled();

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Billing.SubscriptionsEnabled"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Billing.SubscriptionsEnabled instead.", true)]
        OperationResult SubscriptionsEnabled();

        [Obsolete("No replacement for this function. Do not use.", true)]
        OperationResult PropertyEnabled();

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Billing.CartEnabled"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Billing.CartEnabled instead.", true)]
        OperationResult CartEnabled();

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Billing.CartEnabled"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Billing.CartEnabled instead.", true)]
        OperationResult BillingEnabled();

        /// <summary>
        /// Please use <see cref="ISettingsRepository.Basic.SiteTitle"/>, accessible via <see cref="Hood.Core.Engine.Settings"/>
        /// </summary>
        [Obsolete("Please use Engine.Settings.Basics.SiteTitle instead.", true)]
        string GetSiteTitle();

        /// <summary>
        /// Please use <see cref="Hood.Extensions.StringExtensions.ReplaceSiteVariables"/>
        /// </summary>
        [Obsolete("Please use String.ReplaceSiteVariables() instead.", true)]
        string ReplacePlaceholders(string text);

        /// <summary>
        /// Please use <see cref="Hood.Core.Engine.Version"/>
        /// </summary>
        [Obsolete("Please use Engine.Version instead.", true)]
        string GetVersion();

        [Obsolete("No replacement for this function. Do not use.", true)]
        string WysiwygEditorClass { get; }

        /// <summary>
        /// Please use <see cref="Httpcontext.ProcessCaptchaOrThrowAsync"/> />
        /// </summary>
        [Obsolete("Please use Httpcontext.ProcessCaptchaOrThrowAsync() instead.", true)]
        Task ProcessCaptchaOrThrowAsync(HttpRequest request);

        #endregion

    }
}