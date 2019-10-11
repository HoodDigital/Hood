using Hood.Caching;
using Hood.Extensions;
using Hood.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Hood.Services
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly HoodDbContext _db;
        private readonly IConfiguration _config;
        private readonly IHoodCache _cache;

        public SettingsRepository(
            HoodDbContext db,
            IConfiguration config,
            IHoodCache cache)
        {
            _db = db;
            _config = config;
            _cache = cache;
        }

        #region Private Accessors 
        private string Get(string key)
        {
            try
            {
                if (_cache.TryGetValue(key, out Option option))
                {
                    return option.Value;
                }
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
        private void Set(string key, string value)
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
        private void Remove(string key)
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

        #region Get/Set/Delete
        public string this[string key]
        {
            get => Get<string>(key);
            set => Set<string>(value, key);
        }
        public T Get<T>(string key = null)
        {
            if (!key.IsSet())
            {
                key = typeof(T).ToString();
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(Get(key));
            }
            catch
            {
                _cache.Remove(key);
                return default;
            }
        }
        public void Set<T>(T value, string key = null)
        {
            if (!key.IsSet())
            {
                key = typeof(T).ToString();
            }

            try
            {
                Set(key, JsonConvert.SerializeObject(value));
            }
            catch (JsonSerializationException ex)
            {
                throw new Exception($"There was an error serializing option with key: {key}", ex);
            }
        }
        public void Remove<T>(string key = null)
        {
            if (!key.IsSet())
            {
                Remove(typeof(T).ToString());
            }
            else
            {
                Remove(key);
            }
        }
        #endregion

        #region Site Settings
        public BasicSettings Basic => Get<BasicSettings>();
        public IntegrationSettings Integrations => Get<IntegrationSettings>();
        public ContactSettings Contact => Get<ContactSettings>();
        public SeoSettings Seo => Get<SeoSettings>();
        public ContentSettings Content => Get<ContentSettings>();
        public SheduledTaskSettings SheduledTasks => Get<SheduledTaskSettings>();
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
                string userId = Get("Hood.Settings.SiteOwner");
                return _db.UserProfiles.SingleOrDefault(u => u.Id == userId);
            }
        }

        #endregion

        public string ConnectionString => _config.GetConnectionString("DefaultConnection");
        public List<string> LockoutAccessCodes
        {
            get
            {
                string tokens = Basic.LockoutModeTokens;
                if (tokens == null)
                {
                    return new List<string>();
                }

                List<string> allowedCodes = tokens.Split(Environment.NewLine.ToCharArray()).ToList();
                allowedCodes.RemoveAll(str => string.IsNullOrEmpty(str));

                string overrideCode = _config["LockoutMode:OverrideToken"];
                if (overrideCode.IsSet())
                {
                    allowedCodes.Add(overrideCode);
                }

                return allowedCodes;
            }
        }

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
