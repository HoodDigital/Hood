using Hood.Caching;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly HoodDbContext _db;
        private readonly IConfiguration _config;
        private IHoodCache _cache { get; set; }

        public SettingsRepository(
            HoodDbContext db,
            IConfiguration config,
            IHoodCache cache)
        {
            _db = db;
            _config = config;
            _cache = cache;
        }


        #region Get/Set/Delete
        public string this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value);
            }
        }
        public string Get(string key)
        {
            try
            {
                if (_cache.TryGetValue(key, out Option option))
                    return option.Value;
                else
                {
                    option = _db.Options.AsNoTracking().Where(o => o.Id == key).FirstOrDefault();
                    if (option != null)
                    {
                        _cache.Add(key, option);
                        return option.Value;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                _cache.Remove(key);
                return null;
            }
        }
        public void Set(string key, string value)
        {
            try
            {
                Option option = _db.Options.Where(o => o.Id == key).FirstOrDefault();
                if (option == null)
                {
                    option = new Option()
                    {
                        Id = key,
                        Value = value
                    };
                    _db.Options.Add(option);
                }
                else
                {
                    option.Value = value;
                }
                _db.SaveChanges();
                _cache.Remove(key);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException innerException && innerException.Number == 2627)
                {
                    throw new Exception("There is already an option with that key in the database.", ex);
                }
                else
                {
                    throw new Exception($"There was an error saving option with key: {key}", ex);
                }
            }
        }
        public T Get<T>()
        {
            string key = typeof(T).ToString();
            try
            {
                return JsonConvert.DeserializeObject<T>(Get(key));
            }
            catch
            {
                _cache.Remove(key);
                return default(T);
            }
        }
        public void Set<T>(T value)
        {
            string key = typeof(T).ToString();
            try
            {
                Set(key, JsonConvert.SerializeObject(value));
            }
            catch (JsonSerializationException ex)
            {
                throw new Exception($"There was an error serializing option with key: {key}", ex);
            }
        }
        public void Remove<T>()
        {
            Remove(typeof(T).ToString());
        }
        public void Remove(string key)
        {
            _cache.Remove(key);
            Option option = _db.Options.Where(o => o.Id == key).FirstOrDefault();
            if (option != null)
            {
                _db.Entry(option).State = EntityState.Deleted;
                _db.SaveChanges();
            }
        }
        #endregion

        #region Site Settings
        public BasicSettings Basic => Get<BasicSettings>();
        public IntegrationSettings Integrations => Get<IntegrationSettings>();
        public ContactSettings Contact => Get<ContactSettings>();
        public SeoSettings Seo => Get<SeoSettings>();
        public ContentSettings Content => Get<ContentSettings>();
        public PropertySettings Property => Get<PropertySettings>();
        public BillingSettings Billing => Get<BillingSettings>();
        public AccountSettings Account => Get<AccountSettings>();
        public MediaSettings Media => Get<MediaSettings>();
        public MailSettings Mail => Get<MailSettings>();
        public ForumSettings Forum => Get<ForumSettings>();
        public UserProfile SiteOwner
        {
            get
            {
                var userId = Get("Hood.Settings.SiteOwner");
                return _db.UserProfiles.SingleOrDefault(u => u.Id == userId);
            }
        } 

        #endregion

        public string ConnectionString { get { return _config.GetConnectionString("DefaultConnection"); } }
        public List<string> LockoutAccessCodes
        {
            get
            {
                var tokens = Basic.LockoutModeTokens;
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

        #region Obsoletes
        [Obsolete("Please use Engine.Settings.Basics instead.", true)]
        public BasicSettings GetBasicSettings(bool noCache = false) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.IntegrationSettings instead.", true)]
        public IntegrationSettings GetIntegrationSettings(bool noCache = false) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.ContactSettings instead.", true)]
        public ContactSettings GetContactSettings(bool noCache = false) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.SeoSettings instead.", true)]
        public SeoSettings GetSeo(bool noCache = false) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.ContentSettings instead.", true)]
        public ContentSettings GetContentSettings(bool noCache = false) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.PropertySettings instead.", true)]
        public PropertySettings GetPropertySettings(bool noCache = false) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.BillingSettings instead.", true)]
        public BillingSettings GetBillingSettings(bool noCache = false) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.AccountSettings instead.", true)]
        public AccountSettings GetAccountSettings(bool noCache = false) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.Media instead.", true)]
        public MediaSettings GetMediaSettings(bool noCache = false) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.Mail instead.", true)]
        public MailSettings GetMailSettings(bool noCache = false) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.Forum instead.", true)]
        public ForumSettings GetForumSettings(bool noCache = false) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.Billing.CheckStripeOrThrow() instead.", true)]
        public OperationResult StripeEnabled() => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.Billing.CheckPaypalOrThrow() instead.", true)]
        public OperationResult PayPalEnabled() => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.Billing.CheckSubscriptionsOrThrow() instead.", true)]
        public OperationResult SubscriptionsEnabled() => throw new NotImplementedException();

        [Obsolete(null, true)]
        public OperationResult PropertyEnabled() => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.Billing.CheckCartOrThrow() instead.", true)]
        public OperationResult CartEnabled() => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.Billing.CheckBillingOrThrow() instead.", true)]
        public OperationResult BillingEnabled() => throw new NotImplementedException();

        [Obsolete("Please use Engine.Settings.Basics.SiteTitle instead.", true)]
        public string GetSiteTitle() => throw new NotImplementedException();

        [Obsolete("Please use String.ReplaceSiteVariables() instead.", true)]
        public string ReplacePlaceholders(string text) => throw new NotImplementedException();

        [Obsolete("Please use Engine.Version instead.", true)]
        public string GetVersion() => throw new NotImplementedException();

        [Obsolete(null, true)]
        public string WysiwygEditorClass => throw new NotImplementedException();

        [Obsolete("Please use Httpcontext.ProcessCaptchaOrThrowAsync() instead.", true)]
        public Task ProcessCaptchaOrThrowAsync(HttpRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IConfiguration Overrides
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

        #endregion
    }
}
