using Hood.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Hood.Extensions;
using Hood.Infrastructure;
using System.Linq;
using Hood.Models.Api;
using Hood.Caching;

namespace Hood.Services
{
    public class SettingsRepository : ISettingsRepository
    {
        public static object scriptLock = new object();

        private readonly HoodDbContext _db;
        private readonly IConfiguration _config;
        private IHoodCache _cache { get; set; }

        public List<string> LockoutAccessCodes
        {
            get
            {
                var tokens = GetBasicSettings().LockoutModeTokens;
                if (tokens == null)
                    return new List<string>();

                var allowedCodes = tokens.Split(Environment.NewLine.ToCharArray()).ToList();
                allowedCodes.RemoveAll(str => string.IsNullOrEmpty(str));

                string overrideCode = _config["LockoutMode:OverrideToken"];
                if (overrideCode.IsSet())
                    allowedCodes.Add(overrideCode);

                return allowedCodes;
            }
        }

        public string this[string key]
        {
            get
            {
                var res = Get<string>(key);
                if (res != null)
                    return res;
                else return null;
            }
            set
            {
                bool updated = Set(key, value);
            }
        }

        private const string AllSettingsKey = "all_settings";
        private const string BasicSettingsKey = "basic_settings";
        private const string SystemSettingsKey = "system_settings";


        public SettingsRepository(HoodDbContext db,
                                 IConfiguration config,
                                 IHoodCache memoryCache)
        {
            _db = db;
            _config = config;
            _cache = memoryCache;
        }

        public List<Option> AllSettings()
        {
            var options = _db.Options.ToList();
            return options;
        }

        public T Get<T>(string key, bool nocache = false)
        {
            string cacheKey = typeof(Option).ToString() + "." + key;
            try
            {
                Option option = null;
                if (_cache.TryGetValue(cacheKey, out option) && !nocache)
                    return JsonConvert.DeserializeObject<T>(option.Value);
                else
                {
                    option = _db.Options.Where(o => o.Id == key).FirstOrDefault();
                    if (option != null)
                    {
                        _cache.Add(cacheKey, option);
                        return JsonConvert.DeserializeObject<T>(option.Value);
                    }
                    else
                    {
                        return default(T);
                    }
                }
            }
            catch
            {
                _cache.Remove(cacheKey);
                return default(T);
            }
        }
        public bool Set<T>(string key, T value)
        {
            try
            {
                string cacheKey = typeof(Option).ToString() + "." + key;
                Option option = _db.Options.Where(o => o.Id == key).FirstOrDefault();
                if (option == null)
                {
                    option = new Option()
                    {
                        Id = key,
                        Value = JsonConvert.SerializeObject(value)
                    };
                    bool returnVal = Add(option);
                    return returnVal;
                }
                option.Value = JsonConvert.SerializeObject(value);
                bool ret = _db.SaveChanges() == 1;
                if (ret)
                {
                    _cache.Remove(cacheKey);
                }
                return ret;
            }

            catch (DbUpdateException ex)
            {
                SqlException innerException = ex.InnerException as SqlException;
                if (innerException != null && innerException.Number == 2627)
                {
                    throw new Exception("There is already an option with that key in the database.");
                }
                else
                {
                    throw;
                }
            }
        }

        private bool Add(Option option)
        {
            try
            {
                _db.Options.Add(option);
                bool ret = _db.SaveChanges() == 1;
                if (ret)
                {
                    string cacheKey = typeof(Option).ToString() + "." + option.Id;
                    _cache.Add(cacheKey, option);
                }
                return ret;
            }
            catch (DbUpdateException ex)
            {
                SqlException innerException = ex.InnerException as SqlException;
                if (innerException != null && innerException.Number == 2627)
                {
                    throw new Exception("There is already an option with that key in the database.");
                }
                else
                {
                    throw;
                }
            }
        }

        public bool Delete(string key)
        {
            string cacheKey = typeof(Option).ToString() + "." + key;
            _cache.Remove(cacheKey);
            Option option = _db.Options.Where(o => o.Id == key).FirstOrDefault();
            if (option != null)
            {
                _db.Entry(option).State = EntityState.Deleted;
                bool ret = _db.SaveChanges() == 1;
                return ret;
            }
            else
            {
                return true;
            }
        }

        public IConfigurationSection GetSection(string key)
        {
            return null;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return null;
        }

        public IChangeToken GetReloadToken()
        {
            return null;
        }

        public BasicSettings GetBasicSettings(bool noCache = false)
        {
            try
            {
                BasicSettings ret = Get<BasicSettings>("Hood.Settings.Basic", noCache);
                if (ret == null)
                    ret = new BasicSettings();
                return ret;
            }
            catch (Exception)
            {
                Delete("Hood.Settings.Basic");
                return new BasicSettings();
            }
        }

        public IntegrationSettings GetIntegrationSettings(bool noCache = false)
        {
            try
            {
                IntegrationSettings ret = Get<IntegrationSettings>("Hood.Settings.Integrations", noCache);
                if (ret == null)
                    ret = new IntegrationSettings();
                return ret;
            }
            catch (Exception)
            {
                Delete("Hood.Settings.Integrations");
                return new IntegrationSettings();
            }
        }

        public ContactSettings GetContactSettings(bool noCache = false)
        {
            try
            {
                ContactSettings ret = Get<ContactSettings>("Hood.Settings.Contact", noCache);
                if (ret == null)
                    ret = new ContactSettings();
                return ret;
            }
            catch (Exception)
            {
                Delete("Hood.Settings.Contact");
                return new ContactSettings();
            }
        }

        public SeoSettings GetSeo(bool noCache = false)
        {
            try
            {
                SeoSettings ret = Get<SeoSettings>("Hood.Settings.Seo", noCache);
                if (ret == null)
                    ret = new SeoSettings();
                return ret;
            }
            catch (Exception)
            {
                Delete("Hood.Settings.Seo");
                return new SeoSettings();
            }
        }

        public ContentSettings GetContentSettings(bool noCache = false)
        {
            try
            {
                ContentSettings ret = Get<ContentSettings>("Hood.Settings.Content", noCache);
                if (ret == null)
                    ret = new ContentSettings();
                return ret;
            }
            catch (Exception)
            {
                Delete("Hood.Settings.Content");
                return new ContentSettings();
            }
        }

        public PropertySettings GetPropertySettings(bool noCache = false)
        {
            try
            {
                PropertySettings ret = Get<PropertySettings>("Hood.Settings.Property", noCache);
                if (ret == null)
                    ret = new PropertySettings();
                return ret;
            }
            catch (Exception)
            {
                Delete("Hood.Settings.Property");
                return new PropertySettings();
            }
        }

        public BillingSettings GetBillingSettings(bool noCache = false)
        {
            try
            {
                BillingSettings ret = Get<BillingSettings>("Hood.Settings.Billing", noCache);
                if (ret == null)
                    ret = new BillingSettings();
                return ret;
            }
            catch (Exception)
            {
                Delete("Hood.Settings.Billing");
                return new BillingSettings();
            }
        }

        public MediaSettings GetMediaSettings(bool noCache = false)
        {
            try
            {
                MediaSettings ret = Get<MediaSettings>("Hood.Settings.Media", noCache);
                if (ret == null)
                    ret = new MediaSettings();
                return ret;
            }
            catch (Exception)
            {
                Delete("Hood.Settings.Media");
                return new MediaSettings();
            }
        }

        public MailSettings GetMailSettings(bool noCache = false)
        {
            try
            {
                MailSettings ret = Get<MailSettings>("Hood.Settings.Mail", noCache);
                if (ret == null)
                    ret = new MailSettings();
                return ret;
            }
            catch (Exception)
            {
                Delete("Hood.Settings.Mail");
                return new MailSettings();
            }
        }

        public OperationResult StripeEnabled()
        {
            BillingSettings settings = GetBillingSettings();
            if (settings == null)
                return new OperationResult("Stripe is not enabled, please enable it in the administrators area, under Settings > Billing Settings.");
            if (!settings.EnableStripe)
                return new OperationResult("Stripe is not enabled, please enable it in the administrators area, under Settings > Billing Settings.");
            if (!StripeSetup(settings))
                return new OperationResult("Stripe subscriptions are not set up correctly, please ensure you have set the correct settings in the administrators area, under Settings > Billing Settings.");

            return new OperationResult(true);
        }

        public OperationResult PayPalEnabled()
        {
            BillingSettings settings = GetBillingSettings();
            if (settings == null)
                return new OperationResult("PayPal is not enabled, please enable them it the administrators area, under Settings > Billing Settings.");
            if (!settings.EnablePayPal)
                return new OperationResult("PayPal is not enabled, please enable them it the administrators area, under Settings > Billing Settings.");
            if (!PayPalSetup(settings))
                return new OperationResult("PayPal is not set up correctly, please ensure you have set the correct  settings in the administrators area, under Settings > Billing Settings.");

            return new OperationResult(true);
        }

        public OperationResult SubscriptionsEnabled()
        {
            BillingSettings settings = GetBillingSettings();
            if (settings == null)
                return new OperationResult("Subscriptions are not enabled, please enable them in the administrators area, under Settings > Billing Settings.");
            if (!settings.EnableStripe)
                return new OperationResult("Stripe is not enabled, please enable it in the administrators area, under Settings > Billing Settings.");
            if (!settings.EnableSubscriptions)
                return new OperationResult("Subscriptions are not enabled, please enable them in the administrators area, under Settings > Billing Settings.");
            if (!StripeSetup(settings))
                return new OperationResult("Stripe subscriptions are not set up correctly, please ensure you have set the correct  settings in the administrators area, under Settings > Billing Settings.");

            return new OperationResult(true);
        }

        public OperationResult PropertyEnabled()
        {
            PropertySettings settings = GetPropertySettings();
            if (!settings.Enabled)
                return new OperationResult("Property listings are not enabled, please enable them in the administrators area, under Admin Tools > Content Types.");

            return new OperationResult(true);
        }

        public OperationResult CartEnabled()
        {
            BillingSettings settings = GetBillingSettings();
            if (settings == null)
                return new OperationResult("The shopping cart & checkout is not enabled, please enable it in the administrators area, under Settings > Billing Settings.");
            if (!settings.EnableStripe && !settings.EnablePayPal)
                return new OperationResult("Stripe or PayPal are not enabled, please enable one of them in the administrators area, under Settings > Billing Settings.");
            if (!settings.EnableCart)
                return new OperationResult("The shopping cart & checkout is not enabled, please enable it in the administrators area, under Settings > Billing Settings.");
            if (!StripeSetup(settings) && !PayPalSetup(settings))
                return new OperationResult("Stripe or PayPal are not set up correctly, please ensure you have set the correct  settings in the administrators area, under Settings > Billing Settings.");

            return new OperationResult(true);
        }

        public OperationResult BillingEnabled()
        {
            BillingSettings settings = GetBillingSettings();
            if (settings == null)
                return new OperationResult("The shopping cart & checkout is not enabled, please enable it in the administrators area, under Settings > Billing Settings.");
            if (!settings.EnableStripe && !settings.EnablePayPal)
                return new OperationResult("Stripe or PayPal are not enabled, please enable one of them in the administrators area, under Settings > Billing Settings.");
            if (!StripeSetup(settings) && !PayPalSetup(settings))
                return new OperationResult("Stripe or PayPal are not set up correctly, please ensure you have set the correct  settings in the administrators area, under Settings > Billing Settings.");

            return new OperationResult(true);
        }

        private bool StripeSetup(BillingSettings settings)
        {
            if (!settings.StripeLiveKey.IsSet() ||
                !settings.StripeLivePublicKey.IsSet() ||
                !settings.StripeTestKey.IsSet() ||
                !settings.StripeTestPublicKey.IsSet())
                return false;
            return true;
        }
        private bool PayPalSetup(BillingSettings settings)
        {
            if (!settings.StripeLiveKey.IsSet() ||
                !settings.StripeLivePublicKey.IsSet() ||
                !settings.StripeTestKey.IsSet() ||
                !settings.StripeTestPublicKey.IsSet())
                return false;
            return true;
        }

        public string GetSiteTitle()
        {
            var basics = GetBasicSettings();
            if (basics.SiteTitle.IsSet())
                return basics.SiteTitle;
            if (basics.CompanyName.IsSet())
                return basics.CompanyName;
            return null;
        }

        public ContentApi ToContentApi(Content content)
        {
            return new ContentApi(content, this);
        }

        public PropertyListingApi ToPropertyListingApi(PropertyListing property)
        {
            return new PropertyListingApi(property, this);
        }

        public ApplicationUserApi ToApplicationUserApi(ApplicationUser user)
        {
            return new ApplicationUserApi(user, this);
        }

        public string ReplacePlaceholders(string text)
        {
            var basics = GetBasicSettings();
            return text.Replace("{SITETITLE}", GetSiteTitle()).Replace("{COMPANYNAME}", basics.CompanyName).Replace("{SITEOWNERNAME}", basics.OwnerFullName).Replace("{SITEPHONE}", basics.Phone).Replace("{SITEEMAIL}", basics.Email);
        }

        public string GetVersion()
        {
            var version = typeof(SettingsRepository).Assembly.GetName().Version;
            return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }
    }

    public class SettingsRepositoryStub : ISettingsRepository
    {
        public string GetSiteTitle()
        {
            return "Uninstalled Hood Site";
        }
        public string GetVersion()
        {
            var version = typeof(SettingsRepositoryStub).Assembly.GetName().Version;
            return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }
        public string this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public List<string> LockoutAccessCodes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<Option> AllSettings()
        {
            throw new NotImplementedException();
        }

        public OperationResult BillingEnabled()
        {
            throw new NotImplementedException();
        }

        public OperationResult CartEnabled()
        {
            throw new NotImplementedException();
        }

        public bool Delete(string key)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string key, bool nocache = false)
        {
            throw new NotImplementedException();
        }

        public BasicSettings GetBasicSettings(bool noCache = false)
        {
            return new BasicSettings();
        }

        public BillingSettings GetBillingSettings(bool noCache = false)
        {
            return new BillingSettings();
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new NotImplementedException();
        }

        public ContactSettings GetContactSettings(bool noCache = false)
        {
            return new ContactSettings();
        }

        public ContentSettings GetContentSettings(bool noCache = false)
        {
            return new ContentSettings();
        }

        public IntegrationSettings GetIntegrationSettings(bool noCache = false)
        {
            return new IntegrationSettings();
        }

        public MailSettings GetMailSettings(bool noCache = false)
        {
            return new MailSettings();
        }

        public MediaSettings GetMediaSettings(bool noCache = false)
        {
            return new MediaSettings();
        }

        public PropertySettings GetPropertySettings(bool noCache = false)
        {
            return new PropertySettings();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            throw new NotImplementedException();
        }

        public SeoSettings GetSeo(bool noCache = false)
        {
            throw new NotImplementedException();
        }

        public OperationResult PayPalEnabled()
        {
            throw new NotImplementedException();
        }

        public OperationResult PropertyEnabled()
        {
            throw new NotImplementedException();
        }

        public string ReplacePlaceholders(string adminNoficationSubject)
        {
            throw new NotImplementedException();
        }

        public bool Set<T>(string key, T newValue)
        {
            throw new NotImplementedException();
        }

        public OperationResult StripeEnabled()
        {
            throw new NotImplementedException();
        }

        public OperationResult SubscriptionsEnabled()
        {
            throw new NotImplementedException();
        }

        public ApplicationUserApi ToApplicationUserApi(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public ContentApi ToContentApi(Content content)
        {
            throw new NotImplementedException();
        }

        public PropertyListingApi ToPropertyListingApi(PropertyListing property)
        {
            throw new NotImplementedException();
        }
    }

}
