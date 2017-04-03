using Hood.ApiModels;
using Hood.Caching;
using Hood.Core.Infrastructure;
using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Interfaces;
using Hood.IO;
using Hood.Models;
using Hood.Models.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Hood.Services
{
    public class ContentRepository : IContentRepository
    {
        private readonly HoodDbContext _db;
        private readonly IConfiguration _config;
        private readonly ISettingsRepository _settings;
        private readonly IHoodCache _cache;
        private readonly ContentSettings _contentSettings;
        private readonly EventsService _events;
        private readonly IHostingEnvironment _env;

        public ContentRepository(HoodDbContext db,
                                 IHoodCache cache,
                                 IConfiguration config,
                                 ISettingsRepository site,
                                 IHostingEnvironment env,
                                 EventsService events)
        {
            _db = db;
            _config = config;
            _settings = site;
            _cache = cache;
            _events = events;
            _env = env;
            _contentSettings = _settings.GetContentSettings();
        }

        // Content CRUD
        public PagedList<Content> GetPagedContent(ListFilters filters, string type, string category = null, string filter = null, string author = null, bool publishedOnly = true)
        {
            PagedList<Content> content = new PagedList<Content>();
            IOrderedQueryable<Content> contentQuery = GetContent(filters.search, filters.sort, type, category, filter, author, publishedOnly);
            content.Items = contentQuery.ToList().Skip((filters.page - 1) * filters.pageSize).Take(filters.pageSize);
            content.Count = contentQuery.Count();
            content.Pages = content.Count / filters.pageSize;
            if (content.Pages < 1)
                content.Pages = 1;
            if ((content.Pages * filters.pageSize) < content.Count)
            {
                content.Pages++;
            }
            content.CurrentPage = filters.page;
            content.PageSize = filters.pageSize;
            return content;
        }
        private IOrderedQueryable<Content> GetContent(string search, string sort, string type, string category = null, string filter = null, string author = null, bool publishedOnly = true)
        {
            IQueryable<Content> content = _db.Content.Include(p => p.Author)
                                                     .Include(p => p.Media)
                                                     .Include(p => p.Metadata)
                                                     .Include(p => p.Categories).ThenInclude(c => c.Category)
                                                     .Include(p => p.Tags).ThenInclude(t => t.Tag).AsNoTracking();

            // filter posts by type
            if (!string.IsNullOrEmpty(type))
            {
                content = content.Where(c => c.ContentType == type);
            }

            // published?
            if (publishedOnly)
            {
                content = content.Where(p => p.Status == (int)Status.Published);
            }

            // posts by author
            if (!string.IsNullOrEmpty(author))
            {
                content = content.Where(n => n.Author.UserName == author);
            }

            // posts in category
            if (!string.IsNullOrEmpty(category))
            {
                content = content.Where(n => n.Categories.Any(c => c.Category.Slug == category));
            }

            // search the collection

            if (!string.IsNullOrEmpty(search))
            {

                string[] searchTerms = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                content = content.Where(n => searchTerms.Any(s => n.Title != null && n.Title.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Body != null && n.Body.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Excerpt != null && n.Excerpt.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Metadata.Any(m => m.BaseValue != null && m.BaseValue.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)));
            }

            if (!string.IsNullOrEmpty(filter))
            {
                if (filter.StartsWith("Meta:"))
                {
                    filter = filter.Replace("Meta:", "");
                    // search for a specific meta value 
                    if (filter.Contains("="))
                    {
                        var meta = filter.Split('=')[0];
                        var str = filter.Split('=');
                        var value = str[str.Length - 1];
                        content = content.Where(c => c.Metadata.Where(cm => cm.Name == meta && cm.Get<string>() == value).Count() > 0);
                    }
                }
            }

            // sort the collection and then content it.
            IOrderedQueryable<Content> orderedContent = content.OrderByDescending(n => n.PublishDate).ThenBy(n => n.Id);
            if (!string.IsNullOrEmpty(sort))
            {
                if (sort.StartsWith("Meta"))
                {
                    var sortVal = sort.Replace("Meta:", "").Replace(":Desc", "");
                    if (sort.EndsWith("Desc"))
                        orderedContent = content.OrderByDescending(n => n.Metadata.Where(m => m.Name == sortVal).FirstOrDefault().BaseValue);
                    else
                        orderedContent = content.OrderBy(n => n.Metadata.Where(m => m.Name == sortVal).FirstOrDefault().BaseValue);
                }
                else
                {
                    switch (sort)
                    {
                        case "Title":
                            orderedContent = content.OrderBy(n => n.Title);
                            break;
                        case "Date":
                            orderedContent = content.OrderBy(n => n.CreatedOn);
                            break;
                        case "PublishDate":
                            orderedContent = content.OrderBy(n => n.PublishDate);
                            break;
                        case "Views":
                            orderedContent = content.OrderBy(n => n.Views);
                            break;

                        case "TitleDesc":
                            orderedContent = content.OrderByDescending(n => n.Title);
                            break;
                        case "DateDesc":
                            orderedContent = content.OrderByDescending(n => n.CreatedOn);
                            break;
                        case "PublishDateDesc":
                            orderedContent = content.OrderByDescending(n => n.PublishDate);
                            break;
                        case "ViewsDesc":
                            orderedContent = content.OrderByDescending(n => n.Views);
                            break;

                        default:
                            orderedContent = content.OrderByDescending(n => n.PublishDate).ThenBy(n => n.Id);
                            break;
                    }
                }
            }
            return orderedContent;
        }
        public List<Content> GetContentByType(string type, string categorySlug = null, bool publishedOnly = true)
        {
            string cacheKey = typeof(Content).ToString() + ".ByType." + type;
            if (categorySlug.IsSet())
            {
                cacheKey += "-" + categorySlug;
            }
            List<Content> content;
            if (!_cache.TryGetValue(cacheKey, out content))
            {
                var query = _db.Content.Include(p => p.Categories).ThenInclude(c => c.Category)
                                .Include(p => p.Tags).ThenInclude(t => t.Tag)
                                .Include(p => p.Media)
                                .Include(p => p.Metadata)
                                .Include(p => p.Author)
                                .Where(c => c.ContentType == type);
                if (categorySlug.IsSet())
                {
                    query = query.Where(c => c.Categories.Any(cat => cat.Category.Slug == categorySlug));
                }
                if (publishedOnly)
                {
                    query = query.Where(c => c.Status == (int)Status.Published);
                }
                content = query.ToList();
                _cache.Add(cacheKey, content, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(60)));
            }
            return content;
        }
        public Content GetContentByID(int id, bool clearCache = false)
        {
            string cacheKey = typeof(Content).ToString() + ".Single." + id;
            Content content;
            if (!_cache.TryGetValue(cacheKey, out content) || clearCache)
            {
                content = _db.Content.Include(p => p.Categories).ThenInclude(c => c.Category)
                                    .Include(p => p.Tags).ThenInclude(t => t.Tag)
                                    .Include(p => p.Media)
                                    .Include(p => p.Metadata)
                                    .Include(p => p.Author)
                                    .FirstOrDefault(c => c.Id == id);
                if (content == null)
                    return content;
                RefreshMetas(content);
                _cache.Add(cacheKey, content, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
            }
            return content;
        }
        public OperationResult Add(Content content)
        {
            try
            {
                // create the slug
                var generator = new KeyGenerator();
                content.Slug = generator.UrlSlug();
                while (!CheckSlug(content.Slug))
                    content.Slug = generator.UrlSlug();

                _db.Content.Add(content);
                _db.SaveChanges();
                content = GetContentByID(content.Id);
                RefreshMetas(content);
                _events.triggerContentChanged(this);
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message);
            }
        }
        public OperationResult Update(Content content)
        {
            try
            {
                _db.Update(content);
                _db.SaveChanges();
                _events.triggerContentChanged(this);
                return new OperationResult(true);
            }
            catch (DbUpdateException ex)
            {
                return new OperationResult(ex);
            }
        }
        public OperationResult Delete(int id)
        {
            try
            {
                Content content = _db.Content.Where(p => p.Id == id).FirstOrDefault();
                _db.SaveChanges();
                _db.Entry(content).State = EntityState.Deleted;
                _db.SaveChanges();
                _events.triggerContentChanged(this);
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message);
            }
        }
        public OperationResult<Content> SetStatus(int id, Status status)
        {
            try
            {
                Content content = _db.Content.Where(p => p.Id == id).FirstOrDefault();
                content.Status = (int)status;
                _db.SaveChanges();
                _events.triggerContentChanged(this);
                return new OperationResult<Content>(content);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message) as OperationResult<Content>;
            }
        }
        public async Task<OperationResult> DeleteAll(string type)
        {
            try
            {
                _db.Content.Where(c => c.ContentType == type).ForEach(p =>
                {
                    _db.Entry(p).State = EntityState.Deleted;
                });
                await _db.SaveChangesAsync();
                _events.triggerContentChanged(this);
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message);
            }
        }
        public async Task<OperationResult<Content>> AddImage(Content content, ContentMedia media)
        {
            try
            {
                if (content.Media == null)
                    content.Media = new List<ContentMedia>();
                content.Media.Add(media);
                _db.Media.Add(new SiteMedia(media));
                _db.Content.Update(content);
                await _db.SaveChangesAsync();
                _events.triggerContentChanged(this);
                return new OperationResult<Content>(content);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message) as OperationResult<Content>;
            }
        }

        // Content Views
        public List<Content> GetRecent(string type, string categorySlug = null)
        {
            string cacheKey = typeof(Content).ToString() + ".Recent." + type;
            if (categorySlug.IsSet())
                cacheKey += "-" + categorySlug;
            List<Content> content;
            if (!_cache.TryGetValue(cacheKey, out content))
            {
                var query = _db.Content
                                .Include(p => p.Categories).ThenInclude(c => c.Category)
                                .Include(p => p.Media)
                                .Include(p => p.Metadata)
                                .Include(p => p.Author)
                                .AsNoTracking()
                                .Where(p => p.ContentType == type && p.Status == (int)Status.Published);
                if (categorySlug.IsSet())
                {
                    query = query.Where(c => c.Categories.Any(cat => cat.Category.Slug == categorySlug));
                }
                content = query.OrderByDescending(p => p.PublishDate).Take(10).ToList();
                _cache.Add(cacheKey, content, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return content;
        }
        public List<Content> GetFeatured(string type, string categorySlug = null)
        {
            string cacheKey = typeof(Content).ToString() + ".Featured." + type;
            if (categorySlug.IsSet())
                cacheKey += "-" + categorySlug;
            List<Content> content;
            if (!_cache.TryGetValue(cacheKey, out content))
            {
                IQueryable<Content> query = _db.Content
                                .Include(p => p.Categories).ThenInclude(c => c.Category)
                                .Include(p => p.Media)
                                .Include(p => p.Metadata)
                                .Include(p => p.Author)
                                .AsNoTracking()
                                .Where(p => p.ContentType == type && p.Featured && p.Status == (int)Status.Published);
                if (categorySlug.IsSet())
                {
                    query = query.Where(c => c.Categories.Any(cat => cat.Category.Slug == categorySlug));
                }
                content = query.PickRandom(10).ToList();
                _cache.Add(cacheKey, content, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return content;
        }
        public ContentNeighbours GetNeighbourContent(int id, string type, string categorySlug = null)
        {
            string cacheKey = typeof(Content).ToString() + ".Neighbours." + id;
            ContentNeighbours neighbours;
            if (!_cache.TryGetValue(cacheKey, out neighbours))
            {
                // get all the content - sorted by publish date.
                // find the ones either side of the id.
                var all = _db.Content
                         .Include(u => u.Metadata).Where(c => c.Status == (int)Status.Published && c.ContentType == type).AsNoTracking().ToArray();
                var index = Array.FindIndex(all, row => row.Id == id);
                neighbours = new ContentNeighbours();
                neighbours.Next = all.ElementAtOrDefault(index + 1);
                neighbours.Previous = all.ElementAtOrDefault(index - 1);
                _cache.Add(cacheKey, neighbours, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
            }
            return neighbours;
        }

        #region "Tags" 
        public async Task<OperationResult<ContentTag>> AddTag(string value)
        {

            // Ensure it is in title case.
            value = value.Trim().ToTitleCase();

            var tag = _db.ContentTags.SingleOrDefault(t => t.Value == value);
            if (tag == null)
            {
                tag = new ContentTag() { Value = value };
                _db.ContentTags.Add(tag);
                await _db.SaveChangesAsync();
                _events.triggerContentChanged(this);
            }
            return new OperationResult<ContentTag>(tag);

        }
        public async Task<OperationResult> DeleteTag(string value)
        {

            // Ensure it is in title case.
            value = value.ToTitleCase();
            var tag = _db.ContentTags.SingleOrDefault(t => t.Value == value);
            if (tag != null)
            {
                _db.ContentTags.Remove(tag);
                await _db.SaveChangesAsync();
                _events.triggerContentChanged(this);
            }
            return new OperationResult(true);

        }
        #endregion

        #region "Categories" 
        public async Task<ContentCategory> GetCategoryById(int categoryId)
        {
            var category = await _db.ContentCategories.FirstOrDefaultAsync(c => c.ContentCategoryId == categoryId);
            return category;
        }
        public IEnumerable<ContentCategory> GetCategories(int contentId)
        {
            var club = GetContentByID(contentId);
            return club?.Categories?.Select(c => c.Category);
        }
        public async Task<OperationResult<ContentCategory>> AddCategory(string value, string type)
        {
            // Ensure it is in title case.
            value = value.Trim().ToTitleCase();
            var slug = value.ToSeoUrl();
            int counter = 1;
            while (await _db.ContentCategories.CountAsync(cc => cc.Slug == slug && cc.ContentType == type) > 0)
            {
                slug = value.ToSeoUrl() + "-" + counter;
                counter++;
            }
            var category = _db.ContentCategories.SingleOrDefault(t => t.DisplayName == value && t.ContentType == type);
            if (category == null)
            {
                category = new ContentCategory()
                {
                    DisplayName = value,
                    ContentType = type,
                    Slug = slug
                };
                _db.ContentCategories.Add(category);
                await _db.SaveChangesAsync();
                _events.triggerContentChanged(this);
            }
            return new OperationResult<ContentCategory>(category);

        }
        public async Task<OperationResult<ContentCategory>> AddCategory(ContentCategory category)
        {
            // Ensure it is in title case.
            category.DisplayName = category.DisplayName.Trim().ToTitleCase();
            category.Slug = category.DisplayName.ToSeoUrl();
            int counter = 1;
            while (await _db.ContentCategories.CountAsync(cc => cc.Slug == category.Slug) > 0)
            {
                category.Slug = category.DisplayName.ToSeoUrl() + "-" + counter;
                counter++;
            }
            var exists = _db.ContentCategories.SingleOrDefault(t => t.DisplayName == category.DisplayName && t.ContentType == category.ContentType && t.ParentCategoryId == category.ParentCategoryId);
            if (exists == null)
            {
                _db.ContentCategories.Add(category);
                await _db.SaveChangesAsync();
                _events.triggerContentChanged(this);
            }
            return new OperationResult<ContentCategory>(category);
        }
        public async Task<OperationResult> DeleteCategory(int categoryId)
        {
            var category = await _db.ContentCategories.FirstOrDefaultAsync(c => c.ContentCategoryId == categoryId);
            _db.Entry(category).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            _events.triggerContentChanged(this);
            return new OperationResult(true);
        }
        public async Task<OperationResult> UpdateCategory(ContentCategory category)
        {
            _db.Update(category);
            await _db.SaveChangesAsync();
            _events.triggerContentChanged(this);
            return new OperationResult(true);
        }
        #endregion

        // Sitemap Functions
        public List<SitemapPage> GetPages(string categorySlug = null, bool publishedOnly = true)
        {
            List<Content> content = GetContentByType("page", categorySlug, publishedOnly);
            return content.Select(c => new SitemapPage() { PageId = c.Id, Title = c.Title, Url = c.Slug }).ToList();
        }
        public string GetSitemapDocument(IUrlHelper urlHelper)
        {
            List<SitemapNode> nodes = new List<SitemapNode>();

            nodes.Add(new SitemapNode()
            {
                Url = urlHelper.AbsoluteUrl(""),
                Priority = 1,
                Frequency = SitemapFrequency.Daily
            });
            nodes.Add(new SitemapNode()
            {
                Url = urlHelper.AbsoluteUrl("contact"),
                Priority = 0.9,
                Frequency = SitemapFrequency.Never
            });
            foreach (ContentType type in _contentSettings.GetAllowedTypes())
            {
                if (type.IsPublic)
                {
                    nodes.Add(new SitemapNode()
                    {
                        Url = urlHelper.AbsoluteUrl(type.Slug),
                        Frequency = SitemapFrequency.Weekly,
                        Priority = 0.8
                    });
                }
                if (type.HasPage)
                {
                    foreach (var content in GetContentByType(type.Type).OrderByDescending(c => c.PublishDate))
                    {
                        var c = _settings.ToContentApi(content);
                        nodes.Add(new SitemapNode()
                        {
                            Url = urlHelper.AbsoluteUrl(c.Url.TrimStart('/')),
                            LastModified = c.PublishDate,
                            Frequency = SitemapFrequency.Weekly,
                            Priority = 0.7
                        });
                    }
                }
            }

            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XElement root = new XElement(xmlns + "urlset");

            foreach (SitemapNode sitemapNode in nodes)
            {
                XElement urlElement = new XElement(
                    xmlns + "url",
                    new XElement(xmlns + "loc", Uri.EscapeUriString(sitemapNode.Url)),
                    sitemapNode.LastModified == null ? null : new XElement(
                        xmlns + "lastmod",
                        sitemapNode.LastModified.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz")),
                    sitemapNode.Frequency == null ? null : new XElement(
                        xmlns + "changefreq",
                        sitemapNode.Frequency.Value.ToString().ToLowerInvariant()),
                    sitemapNode.Priority == null ? null : new XElement(
                        xmlns + "priority",
                        sitemapNode.Priority.Value.ToString("F1", CultureInfo.InvariantCulture)));
                root.Add(urlElement);
            }

            XDocument document = new XDocument(root);
            return document.ToString();
        }

        // Non Content Related
        public async Task<List<TweetApi>> GetTweets(string name, int count)
        {
            string cacheKey = typeof(TweetApi).ToString() + ".Recent." + name.ToSeoUrl();
            List<TweetApi> tweets = new List<TweetApi>();
            if (_cache.TryGetValue(cacheKey, out tweets))
                return tweets;
            else
            {

                var auth = new LinqToTwitter.ApplicationOnlyAuthorizer
                {
                    CredentialStore = new LinqToTwitter.InMemoryCredentialStore
                    {
                        ConsumerKey = "Aqt8bU9zMSxnfj8hFz0UEA",
                        ConsumerSecret = "HmTlItuFZtLN8zIDqbPrjhPGqywVfZoULqNOSzm29E",
                    }
                };

                await auth.AuthorizeAsync();

                LinqToTwitter.TwitterContext twitterCtx = new LinqToTwitter.TwitterContext(auth);
                ulong sinceID = 1;

                ulong maxID;
                var statusList = new List<LinqToTwitter.Status>();

                var userStatusResponse =
                    (from tweet in twitterCtx.Status
                     where tweet.Type == LinqToTwitter.StatusType.User &&
                           tweet.ScreenName == name &&
                           tweet.SinceID == sinceID &&
                           tweet.Count == count
                     select tweet)
                    .ToList();


                statusList.AddRange(userStatusResponse);

                // first tweet processed on current query
                maxID = userStatusResponse.Min(status => status.StatusID) - 1;

                do
                {
                    // now add sinceID and maxID
                    userStatusResponse =
                        (from tweet in twitterCtx.Status
                         where tweet.Type == LinqToTwitter.StatusType.User &&
                               tweet.ScreenName == name &&
                               tweet.Count == count &&
                               tweet.SinceID == sinceID &&
                               tweet.MaxID == maxID
                         select tweet)
                        .ToList();

                    if (userStatusResponse.Count > 0)
                    {
                        // first tweet processed on current query
                        maxID = userStatusResponse.Min(status => status.StatusID) - 1;

                        statusList.AddRange(userStatusResponse);
                    }
                }
                while (userStatusResponse.Count != 0 && statusList.Count < 30);

                var tws = statusList.Where(t => t.Entities.MediaEntities.Where(me => me.Type == "photo").Count() > 0);
                tweets = tws.Take(6).Select(t => new TweetApi(t)).ToList();
                _cache.Add(cacheKey, tweets);
                return tweets;
            }

        }
        public List<Country> AllCountries()
        {
            string cacheKey = typeof(Country).ToString() + ".List";
            List<Country> countries = new List<Country>();
            if (_cache.TryGetValue(cacheKey, out countries))
                return countries;
            else
            {
                List<Country> dictionary = new List<Country>();
                dictionary.Add(new Country("Afghanistan", "Afghanistan", "AF", "AFG", "4", "AFN", "Afghani"));
                dictionary.Add(new Country("Albania", "Albania", "AL", "ALB", "8", "ALL", "Lek"));
                dictionary.Add(new Country("Algeria", "Algeria", "DZ", "DZA", "12", "DZD", "Algerian Dinar"));
                dictionary.Add(new Country("American Samoa", "American Samoa", "AS", "ASM", "16", "USD", "US Dollar"));
                dictionary.Add(new Country("Andorra", "Andorra", "AD", "AND", "20", "EUR", "Euro"));
                dictionary.Add(new Country("Angola", "Angola", "AO", "AGO", "24", "AOA", "Kwanza"));
                dictionary.Add(new Country("Anguilla", "Anguilla", "AI", "AIA", "660", "XCD", "East Caribbean Dollar"));
                dictionary.Add(new Country("Antarctica", "Antarctica", "AQ", "ATA", "10", "", "No universal currency"));
                dictionary.Add(new Country("Antigua & Barbuda", "Antigua and Barbuda", "AG", "ATG", "28", "XCD", "East Caribbean Dollar"));
                dictionary.Add(new Country("Argentina", "Argentina", "AR", "ARG", "32", "ARS", "Argentine Peso"));
                dictionary.Add(new Country("Armenia", "Armenia", "AM", "ARM", "51", "AMD", "Armenian Dram"));
                dictionary.Add(new Country("Aruba", "Aruba", "AW", "ABW", "533", "AWG", "Aruban Florin"));
                dictionary.Add(new Country("Australia", "Australia", "AU", "AUS", "36", "AUD", "Australian Dollar"));
                dictionary.Add(new Country("Austria", "Austria", "AT", "AUT", "40", "EUR", "Euro"));
                dictionary.Add(new Country("Azerbaijan", "Azerbaijan", "AZ", "AZE", "31", "AZN", "Azerbaijanian Manat"));
                dictionary.Add(new Country("Bahamas", "Bahamas", "BS", "BHS", "44", "BSD", "Bahamian Dollar"));
                dictionary.Add(new Country("Bahrain", "Bahrain", "BH", "BHR", "48", "BHD", "Bahraini Dinar"));
                dictionary.Add(new Country("Bangladesh", "Bangladesh", "BD", "BGD", "50", "BDT", "Taka"));
                dictionary.Add(new Country("Barbados", "Barbados", "BB", "BRB", "52", "BBD", "Barbados Dollar"));
                dictionary.Add(new Country("Belarus", "Belarus", "BY", "BLR", "112", "BYR", "Belarussian Ruble"));
                dictionary.Add(new Country("Belgium", "Belgium", "BE", "BEL", "56", "EUR", "Euro"));
                dictionary.Add(new Country("Belize", "Belize", "BZ", "BLZ", "84", "BZD", "Belize Dollar"));
                dictionary.Add(new Country("Benin", "Benin", "BJ", "BEN", "204", "XOF", "CFA Franc BCEAO"));
                dictionary.Add(new Country("Bermuda", "Bermuda", "BM", "BMU", "60", "BMD", "Bermudian Dollar"));
                dictionary.Add(new Country("Bhutan", "Bhutan", "BT", "BTN", "64", "INR", "Indian Rupee"));
                dictionary.Add(new Country("Bolivia", "Bolivia - Plurinational State of", "BO", "BOL", "68", "BOB", "Boliviano"));
                dictionary.Add(new Country("Caribbean Netherlands", "Bonaire - Sint Eustatius and Saba", "BQ", "BES", "535", "USD", "US Dollar"));
                dictionary.Add(new Country("Bosnia", "Bosnia and Herzegovina", "BA", "BIH", "70", "BAM", "Convertible Mark"));
                dictionary.Add(new Country("Botswana", "Botswana", "BW", "BWA", "72", "BWP", "Pula"));
                dictionary.Add(new Country("Bouvet Island", "Bouvet Island", "BV", "BVT", "74", "NOK", "Norwegian Krone"));
                dictionary.Add(new Country("Brazil", "Brazil", "BR", "BRA", "76", "BRL", "Brazilian Real"));
                dictionary.Add(new Country("British Indian Ocean Territory", "British Indian Ocean Territory", "IO", "IOT", "86", "USD", "US Dollar"));
                dictionary.Add(new Country("Brunei", "Brunei Darussalam", "BN", "BRN", "96", "BND", "Brunei Dollar"));
                dictionary.Add(new Country("Bulgaria", "Bulgaria", "BG", "BGR", "100", "BGN", "Bulgarian Lev"));
                dictionary.Add(new Country("Burkina Faso", "Burkina Faso", "BF", "BFA", "854", "XOF", "CFA Franc BCEAO"));
                dictionary.Add(new Country("Burundi", "Burundi", "BI", "BDI", "108", "BIF", "Burundi Franc"));
                dictionary.Add(new Country("Cambodia", "Cambodia", "KH", "KHM", "116", "KHR", "Riel"));
                dictionary.Add(new Country("Cameroon", "Cameroon", "CM", "CMR", "120", "XAF", "CFA Franc BEAC"));
                dictionary.Add(new Country("Canada", "Canada", "CA", "CAN", "124", "CAD", "Canadian Dollar"));
                dictionary.Add(new Country("Cape Verde", "Cape Verde", "CV", "CPV", "132", "CVE", "Cabo Verde Escudo"));
                dictionary.Add(new Country("Cayman Islands", "Cayman Islands", "KY", "CYM", "136", "KYD", "Cayman Islands Dollar"));
                dictionary.Add(new Country("Central African Republic", "Central African Republic", "CF", "CAF", "140", "XAF", "CFA Franc BEAC"));
                dictionary.Add(new Country("Chad", "Chad", "TD", "TCD", "148", "XAF", "CFA Franc BEAC"));
                dictionary.Add(new Country("Chile", "Chile", "CL", "CHL", "152", "CLP", "Chilean Peso"));
                dictionary.Add(new Country("China", "China", "CN", "CHN", "156", "CNY", "Yuan Renminbi"));
                dictionary.Add(new Country("Christmas Island", "Christmas Island", "CX", "CXR", "162", "AUD", "Australian Dollar"));
                dictionary.Add(new Country("Cocos (Keeling) Islands", "Cocos (Keeling) Islands", "CC", "CCK", "166", "AUD", "Australian Dollar"));
                dictionary.Add(new Country("Colombia", "Colombia", "CO", "COL", "170", "COP", "Colombian Peso"));
                dictionary.Add(new Country("Comoros", "Comoros", "KM", "COM", "174", "KMF", "Comoro Franc"));
                dictionary.Add(new Country("Congo - Brazzaville", "Congo", "CG", "COG", "178", "XAF", "CFA Franc BEAC"));
                dictionary.Add(new Country("Congo - Kinshasa", "Congo - the Democratic Republic of the", "CD", "COD", "180", "", ""));
                dictionary.Add(new Country("Cook Islands", "Cook Islands", "CK", "COK", "184", "NZD", "New Zealand Dollar"));
                dictionary.Add(new Country("Costa Rica", "Costa Rica", "CR", "CRI", "188", "CRC", "Costa Rican Colon"));
                dictionary.Add(new Country("Croatia", "Croatia", "HR", "HRV", "191", "HRK", "Croatian Kuna"));
                dictionary.Add(new Country("Cuba", "Cuba", "CU", "CUB", "192", "CUP", "Cuban Peso"));
                dictionary.Add(new Country("Curaçao", "Curaçao", "CW", "CUW", "531", "ANG", "Netherlands Antillean Guilder"));
                dictionary.Add(new Country("Cyprus", "Cyprus", "CY", "CYP", "196", "EUR", "Euro"));
                dictionary.Add(new Country("Czech Republic", "Czech Republic", "CZ", "CZE", "203", "CZK", "Czech Koruna"));
                dictionary.Add(new Country("Côte d’Ivoire", "Côte d'Ivoire", "CI", "CIV", "384", "XOF", "CFA Franc BCEAO"));
                dictionary.Add(new Country("Denmark", "Denmark", "DK", "DNK", "208", "DKK", "Danish Krone"));
                dictionary.Add(new Country("Djibouti", "Djibouti", "DJ", "DJI", "262", "DJF", "Djibouti Franc"));
                dictionary.Add(new Country("Dominica", "Dominica", "DM", "DMA", "212", "XCD", "East Caribbean Dollar"));
                dictionary.Add(new Country("Dominican Republic", "Dominican Republic", "DO", "DOM", "214", "DOP", "Dominican Peso"));
                dictionary.Add(new Country("Ecuador", "Ecuador", "EC", "ECU", "218", "USD", "US Dollar"));
                dictionary.Add(new Country("Egypt", "Egypt", "EG", "EGY", "818", "EGP", "Egyptian Pound"));
                dictionary.Add(new Country("El Salvador", "El Salvador", "SV", "SLV", "222", "USD", "US Dollar"));
                dictionary.Add(new Country("Equatorial Guinea", "Equatorial Guinea", "GQ", "GNQ", "226", "XAF", "CFA Franc BEAC"));
                dictionary.Add(new Country("Eritrea", "Eritrea", "ER", "ERI", "232", "ERN", "Nakfa"));
                dictionary.Add(new Country("Estonia", "Estonia", "EE", "EST", "233", "EUR", "Euro"));
                dictionary.Add(new Country("Ethiopia", "Ethiopia", "ET", "ETH", "231", "ETB", "Ethiopian Birr"));
                dictionary.Add(new Country("Falkland Islands", "Falkland Islands (Malvinas)", "FK", "FLK", "238", "FKP", "Falkland Islands Pound"));
                dictionary.Add(new Country("Faroe Islands", "Faroe Islands", "FO", "FRO", "234", "DKK", "Danish Krone"));
                dictionary.Add(new Country("Fiji", "Fiji", "FJ", "FJI", "242", "FJD", "Fiji Dollar"));
                dictionary.Add(new Country("Finland", "Finland", "FI", "FIN", "246", "EUR", "Euro"));
                dictionary.Add(new Country("France", "France", "FR", "FRA", "250", "EUR", "Euro"));
                dictionary.Add(new Country("French Guiana", "French Guiana", "GF", "GUF", "254", "EUR", "Euro"));
                dictionary.Add(new Country("French Polynesia", "French Polynesia", "PF", "PYF", "258", "XPF", "CFP Franc"));
                dictionary.Add(new Country("French Southern Territories", "French Southern Territories", "TF", "ATF", "260", "EUR", "Euro"));
                dictionary.Add(new Country("Gabon", "Gabon", "GA", "GAB", "266", "XAF", "CFA Franc BEAC"));
                dictionary.Add(new Country("Gambia", "Gambia", "GM", "GMB", "270", "GMD", "Dalasi"));
                dictionary.Add(new Country("Georgia", "Georgia", "GE", "GEO", "268", "GEL", "Lari"));
                dictionary.Add(new Country("Germany", "Germany", "DE", "DEU", "276", "EUR", "Euro"));
                dictionary.Add(new Country("Ghana", "Ghana", "GH", "GHA", "288", "GHS", "Ghana Cedi"));
                dictionary.Add(new Country("Gibraltar", "Gibraltar", "GI", "GIB", "292", "GIP", "Gibraltar Pound"));
                dictionary.Add(new Country("Greece", "Greece", "GR", "GRC", "300", "EUR", "Euro"));
                dictionary.Add(new Country("Greenland", "Greenland", "GL", "GRL", "304", "DKK", "Danish Krone"));
                dictionary.Add(new Country("Grenada", "Grenada", "GD", "GRD", "308", "XCD", "East Caribbean Dollar"));
                dictionary.Add(new Country("Guadeloupe", "Guadeloupe", "GP", "GLP", "312", "EUR", "Euro"));
                dictionary.Add(new Country("Guam", "Guam", "GU", "GUM", "316", "USD", "US Dollar"));
                dictionary.Add(new Country("Guatemala", "Guatemala", "GT", "GTM", "320", "GTQ", "Quetzal"));
                dictionary.Add(new Country("Guernsey", "Guernsey", "GG", "GGY", "831", "GBP", "Pound Sterling"));
                dictionary.Add(new Country("Guinea", "Guinea", "GN", "GIN", "324", "GNF", "Guinea Franc"));
                dictionary.Add(new Country("Guinea-Bissau", "Guinea-Bissau", "GW", "GNB", "624", "XOF", "CFA Franc BCEAO"));
                dictionary.Add(new Country("Guyana", "Guyana", "GY", "GUY", "328", "GYD", "Guyana Dollar"));
                dictionary.Add(new Country("Haiti", "Haiti", "HT", "HTI", "332", "USD", "US Dollar"));
                dictionary.Add(new Country("Heard & McDonald Islands", "Heard Island and McDonald Islands", "HM", "HMD", "334", "AUD", "Australian Dollar"));
                dictionary.Add(new Country("Vatican City", "Holy See (Vatican City State)", "VA", "VAT", "336", "EUR", "Euro"));
                dictionary.Add(new Country("Honduras", "Honduras", "HN", "HND", "340", "HNL", "Lempira"));
                dictionary.Add(new Country("Hong Kong", "Hong Kong", "HK", "HKG", "344", "HKD", "Hong Kong Dollar"));
                dictionary.Add(new Country("Hungary", "Hungary", "HU", "HUN", "348", "HUF", "Forint"));
                dictionary.Add(new Country("Iceland", "Iceland", "IS", "ISL", "352", "ISK", "Iceland Krona"));
                dictionary.Add(new Country("India", "India", "IN", "IND", "356", "INR", "Indian Rupee"));
                dictionary.Add(new Country("Indonesia", "Indonesia", "ID", "IDN", "360", "IDR", "Rupiah"));
                dictionary.Add(new Country("Iran", "Iran - Islamic Republic of", "IR", "IRN", "364", "IRR", "Iranian Rial"));
                dictionary.Add(new Country("Iraq", "Iraq", "IQ", "IRQ", "368", "IQD", "Iraqi Dinar"));
                dictionary.Add(new Country("Ireland", "Ireland", "IE", "IRL", "372", "EUR", "Euro"));
                dictionary.Add(new Country("Isle of Man", "Isle of Man", "IM", "IMN", "833", "GBP", "Pound Sterling"));
                dictionary.Add(new Country("Israel", "Israel", "IL", "ISR", "376", "ILS", "New Israeli Sheqel"));
                dictionary.Add(new Country("Italy", "Italy", "IT", "ITA", "380", "EUR", "Euro"));
                dictionary.Add(new Country("Jamaica", "Jamaica", "JM", "JAM", "388", "JMD", "Jamaican Dollar"));
                dictionary.Add(new Country("Japan", "Japan", "JP", "JPN", "392", "JPY", "Yen"));
                dictionary.Add(new Country("Jersey", "Jersey", "JE", "JEY", "832", "GBP", "Pound Sterling"));
                dictionary.Add(new Country("Jordan", "Jordan", "JO", "JOR", "400", "JOD", "Jordanian Dinar"));
                dictionary.Add(new Country("Kazakhstan", "Kazakhstan", "KZ", "KAZ", "398", "KZT", "Tenge"));
                dictionary.Add(new Country("Kenya", "Kenya", "KE", "KEN", "404", "KES", "Kenyan Shilling"));
                dictionary.Add(new Country("Kiribati", "Kiribati", "KI", "KIR", "296", "AUD", "Australian Dollar"));
                dictionary.Add(new Country("North Korea", "Korea - Democratic People's Republic of", "KP", "PRK", "408", "KPW", "North Korean Won"));
                dictionary.Add(new Country("South Korea", "Korea - Republic of", "KR", "KOR", "410", "KRW", "Won"));
                dictionary.Add(new Country("Kuwait", "Kuwait", "KW", "KWT", "414", "KWD", "Kuwaiti Dinar"));
                dictionary.Add(new Country("Kyrgyzstan", "Kyrgyzstan", "KG", "KGZ", "417", "KGS", "Som"));
                dictionary.Add(new Country("Laos", "Lao People's Democratic Republic", "LA", "LAO", "418", "LAK", "Kip"));
                dictionary.Add(new Country("Latvia", "Latvia", "LV", "LVA", "428", "EUR", "Euro"));
                dictionary.Add(new Country("Lebanon", "Lebanon", "LB", "LBN", "422", "LBP", "Lebanese Pound"));
                dictionary.Add(new Country("Lesotho", "Lesotho", "LS", "LSO", "426", "ZAR", "Rand"));
                dictionary.Add(new Country("Liberia", "Liberia", "LR", "LBR", "430", "LRD", "Liberian Dollar"));
                dictionary.Add(new Country("Libya", "Libya", "LY", "LBY", "434", "LYD", "Libyan Dinar"));
                dictionary.Add(new Country("Liechtenstein", "Liechtenstein", "LI", "LIE", "438", "CHF", "Swiss Franc"));
                dictionary.Add(new Country("Lithuania", "Lithuania", "LT", "LTU", "440", "EUR", "Euro"));
                dictionary.Add(new Country("Luxembourg", "Luxembourg", "LU", "LUX", "442", "EUR", "Euro"));
                dictionary.Add(new Country("Macau", "Macao", "MO", "MAC", "446", "MOP", "Pataca"));
                dictionary.Add(new Country("Macedonia", "Macedonia - the Former Yugoslav Republic of", "MK", "MKD", "807", "MKD", "Denar"));
                dictionary.Add(new Country("Madagascar", "Madagascar", "MG", "MDG", "450", "MGA", "Malagasy Ariary"));
                dictionary.Add(new Country("Malawi", "Malawi", "MW", "MWI", "454", "MWK", "Kwacha"));
                dictionary.Add(new Country("Malaysia", "Malaysia", "MY", "MYS", "458", "MYR", "Malaysian Ringgit"));
                dictionary.Add(new Country("Maldives", "Maldives", "MV", "MDV", "462", "MVR", "Rufiyaa"));
                dictionary.Add(new Country("Mali", "Mali", "ML", "MLI", "466", "XOF", "CFA Franc BCEAO"));
                dictionary.Add(new Country("Malta", "Malta", "MT", "MLT", "470", "EUR", "Euro"));
                dictionary.Add(new Country("Marshall Islands", "Marshall Islands", "MH", "MHL", "584", "USD", "US Dollar"));
                dictionary.Add(new Country("Martinique", "Martinique", "MQ", "MTQ", "474", "EUR", "Euro"));
                dictionary.Add(new Country("Mauritania", "Mauritania", "MR", "MRT", "478", "MRO", "Ouguiya"));
                dictionary.Add(new Country("Mauritius", "Mauritius", "MU", "MUS", "480", "MUR", "Mauritius Rupee"));
                dictionary.Add(new Country("Mayotte", "Mayotte", "YT", "MYT", "175", "EUR", "Euro"));
                dictionary.Add(new Country("Mexico", "Mexico", "MX", "MEX", "484", "MXN", "Mexican Peso"));
                dictionary.Add(new Country("Micronesia", "Micronesia - Federated States of", "FM", "FSM", "583", "USD", "US Dollar"));
                dictionary.Add(new Country("Moldova", "Moldova - Republic of", "MD", "MDA", "498", "MDL", "Moldovan Leu"));
                dictionary.Add(new Country("Monaco", "Monaco", "MC", "MCO", "492", "EUR", "Euro"));
                dictionary.Add(new Country("Mongolia", "Mongolia", "MN", "MNG", "496", "MNT", "Tugrik"));
                dictionary.Add(new Country("Montenegro", "Montenegro", "ME", "MNE", "499", "EUR", "Euro"));
                dictionary.Add(new Country("Montserrat", "Montserrat", "MS", "MSR", "500", "XCD", "East Caribbean Dollar"));
                dictionary.Add(new Country("Morocco", "Morocco", "MA", "MAR", "504", "MAD", "Moroccan Dirham"));
                dictionary.Add(new Country("Mozambique", "Mozambique", "MZ", "MOZ", "508", "MZN", "Mozambique Metical"));
                dictionary.Add(new Country("Myanmar", "Myanmar", "MM", "MMR", "104", "MMK", "Kyat"));
                dictionary.Add(new Country("Namibia", "Namibia", "NA", "NAM", "516", "ZAR", "Rand"));
                dictionary.Add(new Country("Nauru", "Nauru", "NR", "NRU", "520", "AUD", "Australian Dollar"));
                dictionary.Add(new Country("Nepal", "Nepal", "NP", "NPL", "524", "NPR", "Nepalese Rupee"));
                dictionary.Add(new Country("Netherlands", "Netherlands", "NL", "NLD", "528", "EUR", "Euro"));
                dictionary.Add(new Country("New Caledonia", "New Caledonia", "NC", "NCL", "540", "XPF", "CFP Franc"));
                dictionary.Add(new Country("New Zealand", "New Zealand", "NZ", "NZL", "554", "NZD", "New Zealand Dollar"));
                dictionary.Add(new Country("Nicaragua", "Nicaragua", "NI", "NIC", "558", "NIO", "Cordoba Oro"));
                dictionary.Add(new Country("Niger", "Niger", "NE", "NER", "562", "XOF", "CFA Franc BCEAO"));
                dictionary.Add(new Country("Nigeria", "Nigeria", "NG", "NGA", "566", "NGN", "Naira"));
                dictionary.Add(new Country("Niue", "Niue", "NU", "NIU", "570", "NZD", "New Zealand Dollar"));
                dictionary.Add(new Country("Norfolk Island", "Norfolk Island", "NF", "NFK", "574", "AUD", "Australian Dollar"));
                dictionary.Add(new Country("Northern Mariana Islands", "Northern Mariana Islands", "MP", "MNP", "580", "USD", "US Dollar"));
                dictionary.Add(new Country("Norway", "Norway", "NO", "NOR", "578", "NOK", "Norwegian Krone"));
                dictionary.Add(new Country("Oman", "Oman", "OM", "OMN", "512", "OMR", "Rial Omani"));
                dictionary.Add(new Country("Pakistan", "Pakistan", "PK", "PAK", "586", "PKR", "Pakistan Rupee"));
                dictionary.Add(new Country("Palau", "Palau", "PW", "PLW", "585", "USD", "US Dollar"));
                dictionary.Add(new Country("Palestine", "State of Palestine", " PS", "PSE", "275", "", "No universal currency"));
                dictionary.Add(new Country("Panama", "Panama", "PA", "PAN", "591", "USD", "US Dollar"));
                dictionary.Add(new Country("Papua New Guinea", "Papua New Guinea", "PG", "PNG", "598", "PGK", "Kina"));
                dictionary.Add(new Country("Paraguay", "Paraguay", "PY", "PRY", "600", "PYG", "Guarani"));
                dictionary.Add(new Country("Peru", "Peru", "PE", "PER", "604", "PEN", "Nuevo Sol"));
                dictionary.Add(new Country("Philippines", "Philippines", "PH", "PHL", "608", "PHP", "Philippine Peso"));
                dictionary.Add(new Country("Pitcairn Islands", "Pitcairn", "PN", "PCN", "612", "NZD", "New Zealand Dollar"));
                dictionary.Add(new Country("Poland", "Poland", "PL", "POL", "616", "PLN", "Zloty"));
                dictionary.Add(new Country("Portugal", "Portugal", "PT", "PRT", "620", "EUR", "Euro"));
                dictionary.Add(new Country("Puerto Rico", "Puerto Rico", "PR", "PRI", "630", "USD", "US Dollar"));
                dictionary.Add(new Country("Qatar", "Qatar", "QA", "QAT", "634", "QAR", "Qatari Rial"));
                dictionary.Add(new Country("Romania", "Romania", "RO", "ROU", "642", "RON", "New Romanian Leu"));
                dictionary.Add(new Country("Russia", "Russian Federation", "RU", "RUS", "643", "RUB", "Russian Ruble"));
                dictionary.Add(new Country("Rwanda", "Rwanda", "RW", "RWA", "646", "RWF", "Rwanda Franc"));
                dictionary.Add(new Country("Réunion", "Réunion", "RE", "REU", "638", "EUR", "Euro"));
                dictionary.Add(new Country("St. Barthélemy", "Saint Barthélemy", "BL", "BLM", "652", "EUR", "Euro"));
                dictionary.Add(new Country("St. Helena", "Ascension and Tristan da Cunha Saint Helena", "SH", "SHN", "654", "SHP", "Saint Helena Pound"));
                dictionary.Add(new Country("St. Kitts & Nevis", "Saint Kitts and Nevis", "KN", "KNA", "659", "XCD", "East Caribbean Dollar"));
                dictionary.Add(new Country("St. Lucia", "Saint Lucia", "LC", "LCA", "662", "XCD", "East Caribbean Dollar"));
                dictionary.Add(new Country("St. Martin", "Saint Martin (French part)", "MF", "MAF", "663", "EUR", "Euro"));
                dictionary.Add(new Country("St. Pierre & Miquelon", "Saint Pierre and Miquelon", "PM", "SPM", "666", "EUR", "Euro"));
                dictionary.Add(new Country("St. Vincent & Grenadines", "Saint Vincent and the Grenadines", "VC", "VCT", "670", "XCD", "East Caribbean Dollar"));
                dictionary.Add(new Country("Samoa", "Samoa", "WS", "WSM", "882", "WST", "Tala"));
                dictionary.Add(new Country("San Marino", "San Marino", "SM", "SMR", "674", "EUR", "Euro"));
                dictionary.Add(new Country("São Tomé & Príncipe", "Sao Tome and Principe", "ST", "STP", "678", "STD", "Dobra"));
                dictionary.Add(new Country("Saudi Arabia", "Saudi Arabia", "SA", "SAU", "682", "SAR", "Saudi Riyal"));
                dictionary.Add(new Country("Senegal", "Senegal", "SN", "SEN", "686", "XOF", "CFA Franc BCEAO"));
                dictionary.Add(new Country("Serbia", "Serbia", "RS", "SRB", "688", "RSD", "Serbian Dinar"));
                dictionary.Add(new Country("Seychelles", "Seychelles", "SC", "SYC", "690", "SCR", "Seychelles Rupee"));
                dictionary.Add(new Country("Sierra Leone", "Sierra Leone", "SL", "SLE", "694", "SLL", "Leone"));
                dictionary.Add(new Country("Singapore", "Singapore", "SG", "SGP", "702", "SGD", "Singapore Dollar"));
                dictionary.Add(new Country("Sint Maarten", "Sint Maarten (Dutch part)", "SX", "SXM", "534", "ANG", "Netherlands Antillean Guilder"));
                dictionary.Add(new Country("Slovakia", "Slovakia", "SK", "SVK", "703", "EUR", "Euro"));
                dictionary.Add(new Country("Slovenia", "Slovenia", "SI", "SVN", "705", "EUR", "Euro"));
                dictionary.Add(new Country("Solomon Islands", "Solomon Islands", "SB", "SLB", "90", "SBD", "Solomon Islands Dollar"));
                dictionary.Add(new Country("Somalia", "Somalia", "SO", "SOM", "706", "SOS", "Somali Shilling"));
                dictionary.Add(new Country("South Africa", "South Africa", "ZA", "ZAF", "710", "ZAR", "Rand"));
                dictionary.Add(new Country("South Georgia & South Sandwich Islands", "South Georgia and the South Sandwich Islands", "GS", "SGS", "239", "", "No universal currency"));
                dictionary.Add(new Country("South Sudan", "South Sudan", "SS", "SSD", "728", "SSP", "South Sudanese Pound"));
                dictionary.Add(new Country("Spain", "Spain", "ES", "ESP", "724", "EUR", "Euro"));
                dictionary.Add(new Country("Sri Lanka", "Sri Lanka", "LK", "LKA", "144", "LKR", "Sri Lanka Rupee"));
                dictionary.Add(new Country("Sudan", "Sudan", "SD", "SDN", "729", "SDG", "Sudanese Pound"));
                dictionary.Add(new Country("Suriname", "Suriname", "SR", "SUR", "740", "SRD", "Surinam Dollar"));
                dictionary.Add(new Country("Svalbard & Jan Mayen", "Svalbard and Jan Mayen", "SJ", "SJM", "744", "NOK", "Norwegian Krone"));
                dictionary.Add(new Country("Swaziland", "Swaziland", "SZ", "SWZ", "748", "SZL", "Lilangeni"));
                dictionary.Add(new Country("Sweden", "Sweden", "SE", "SWE", "752", "SEK", "Swedish Krona"));
                dictionary.Add(new Country("Switzerland", "Switzerland", "CH", "CHE", "756", "CHF", "Swiss Franc"));
                dictionary.Add(new Country("Syria", "Syrian Arab Republic", "SY", "SYR", "760", "SYP", "Syrian Pound"));
                dictionary.Add(new Country("Taiwan", "Taiwan", "TW", "TWN", "158", "TWD", "New Taiwan Dollar"));
                dictionary.Add(new Country("Tajikistan", "Tajikistan", "TJ", "TJK", "762", "TJS", "Somoni"));
                dictionary.Add(new Country("Tanzania", " United Republic of Tanzania", " TZ", "TZA", "834", "TZS", "Tanzanian Shilling"));
                dictionary.Add(new Country("Thailand", "Thailand", "TH", "THA", "764", "THB", "Baht"));
                dictionary.Add(new Country("Timor-Leste", "Timor-Leste", "TL", "TLS", "626", "USD", "US Dollar"));
                dictionary.Add(new Country("Togo", "Togo", "TG", "TGO", "768", "XOF", "CFA Franc BCEAO"));
                dictionary.Add(new Country("Tokelau", "Tokelau", "TK", "TKL", "772", "NZD", "New Zealand Dollar"));
                dictionary.Add(new Country("Tonga", "Tonga", "TO", "TON", "776", "TOP", "Pa’anga"));
                dictionary.Add(new Country("Trinidad & Tobago", "Trinidad and Tobago", "TT", "TTO", "780", "TTD", "Trinidad and Tobago Dollar"));
                dictionary.Add(new Country("Tunisia", "Tunisia", "TN", "TUN", "788", "TND", "Tunisian Dinar"));
                dictionary.Add(new Country("Turkey", "Turkey", "TR", "TUR", "792", "TRY", "Turkish Lira"));
                dictionary.Add(new Country("Turkmenistan", "Turkmenistan", "TM", "TKM", "795", "TMT", "Turkmenistan New Manat"));
                dictionary.Add(new Country("Turks & Caicos Islands", "Turks and Caicos Islands", "TC", "TCA", "796", "USD", "US Dollar"));
                dictionary.Add(new Country("Tuvalu", "Tuvalu", "TV", "TUV", "798", "AUD", "Australian Dollar"));
                dictionary.Add(new Country("Uganda", "Uganda", "UG", "UGA", "800", "UGX", "Uganda Shilling"));
                dictionary.Add(new Country("Ukraine", "Ukraine", "UA", "UKR", "804", "UAH", "Hryvnia"));
                dictionary.Add(new Country("United Arab Emirates", "United Arab Emirates", "AE", "ARE", "784", "AED", "UAE Dirham"));
                dictionary.Add(new Country("United Kingdom", "United Kingdom", "GB", "GBR", "826", "GBP", "Pound Sterling"));
                dictionary.Add(new Country("United States", "United States of America", "US", "USA", "840", "USD", "US Dollar"));
                dictionary.Add(new Country("U.S. Outlying Islands", "United States Minor Outlying Islands", "UM", "UMI", "581", "USD", "US Dollar"));
                dictionary.Add(new Country("Uruguay", "Uruguay", "UY", "URY", "858", "UYU", "Peso Uruguayo"));
                dictionary.Add(new Country("Uzbekistan", "Uzbekistan", "UZ", "UZB", "860", "UZS", "Uzbekistan Sum"));
                dictionary.Add(new Country("Vanuatu", "Vanuatu", "VU", "VUT", "548", "VUV", "Vatu"));
                dictionary.Add(new Country("Venezuela", "Bolivarian Republic of Venezuela", "VE", "VEN", "862", "VEF", "Bolivar"));
                dictionary.Add(new Country("Vietnam", "Viet Nam", "VN", "VNM", "704", "VND", "Dong"));
                dictionary.Add(new Country("British Virgin Islands", "British Virgin Islands", "VG", "VGB", "92", "USD", "US Dollar"));
                dictionary.Add(new Country("U.S. Virgin Islands", " U.S. Virgin Islands", " VI", "VIR", "850", "USD", "US Dollar"));
                dictionary.Add(new Country("Wallis & Futuna", "Wallis and Futuna", "WF", "WLF", "876", "XPF", "CFP Franc"));
                dictionary.Add(new Country("Western Sahara", "Western Sahara", "EH", "ESH", "732", "MAD", "Moroccan Dirham"));
                dictionary.Add(new Country("Yemen", "Yemen", "YE", "YEM", "887", "YER", "Yemeni Rial"));
                dictionary.Add(new Country("Zambia", "Zambia", "ZM", "ZMB", "894", "ZMW", "Zambian Kwacha"));
                dictionary.Add(new Country("Zimbabwe", "Zimbabwe", "ZW", "ZWE", "716", "ZWL", "Zimbabwe Dollar"));
                dictionary.Add(new Country("Åland Islands", "Åland Islands", "AX", "ALA", "248", "EUR", "Euro"));
                dictionary = dictionary.OrderBy(c => c.Name).ToList();
                _cache.Add(cacheKey, dictionary);
                return dictionary;
            }
        }
        public Country GetCountry(string name)
        {
            var country = AllCountries().Where(c => c.Name == name).FirstOrDefault();
            return country;
        }

        public void UpdateTemplateMetas(Content content, List<string> newMetas)
        {
            // iterate through content metas that start with Template_
            // if it exists in new, leave, if it doesnt remove 
            List<ContentMeta> toRemove = new List<ContentMeta>();
            foreach (ContentMeta cm in content.Metadata)
            {
                if (cm.Name.StartsWith("Template.") && !newMetas.Contains(cm.Name))
                {
                    _db.Entry(cm).State = EntityState.Deleted;
                    toRemove.Add(cm);
                }
            }
            foreach (ContentMeta cm in toRemove)
            {
                content.Metadata.Remove(cm);
            }

            // iterate through new metas 
            // if it doesnt exist in content.Metas, add to content.Metas
            if (newMetas != null)
                foreach (string meta in newMetas)
                {
                    if (!content.HasMeta(meta))
                    {
                        content.Metadata.Add(new ContentMeta()
                        {
                            ContentId = content.Id,
                            Name = meta,
                            Type = "System.String",
                            BaseValue = JsonConvert.SerializeObject("")
                        });
                    }
                }

        }
        public void RefreshMetas(Content content)
        {
            var _contentSettings = _settings.GetContentSettings();
            foreach (CustomField field in _contentSettings.GetContentType(content.ContentType).CustomFields)
            {
                if (content.HasMeta(field.Name))
                {
                    // ensure it has the correct type.
                    var meta = content.GetMeta(field.Name);
                    if (meta.Type != field.Type)
                        meta.Type = field.Type;
                }
                else
                {
                    // Add it...
                    content.AddMeta(new ContentMeta()
                    {
                        ContentId = content.Id,
                        Name = field.Name,
                        Type = field.Type,
                        BaseValue = JsonConvert.SerializeObject(field.Default)
                    });
                }
            }
            _db.SaveChanges();
            _events.triggerContentChanged(this);
        }
        public void RefreshAllMetas()
        {
            var _contentSettings = _settings.GetContentSettings();
            foreach (var content in _db.Content.Include(p => p.Metadata).ToList())
            {
                RefreshMetas(content);
                var currentTemplate = content.GetMeta("Settings.Template");
                if (currentTemplate != null)
                {

                    // delete all template metas that do not exist in the new template, and add any that are missing
                    var type = _contentSettings.GetContentType(content.ContentType);
                    List<string> newMetas = GetMetasForTemplate(currentTemplate.GetStringValue(), type.TemplateFolder);
                    UpdateTemplateMetas(content, newMetas);
                }
            }
            _db.SaveChanges();
        }
        public List<string> GetMetasForTemplate(string templateName, string folder)
        {
            templateName = templateName.Replace("Meta:", "");

            // get the right template file (from theme or if it doesnt appear there from base)
            string templatePath = _env.ContentRootPath + "\\Themes\\" + _settings["Hood.Settings.Theme"] + "\\Views\\" + folder + "\\" + templateName + ".cshtml";
            if (!System.IO.File.Exists(templatePath))
                templatePath = _env.ContentRootPath + "\\Views\\" + folder + "\\" + templateName + ".cshtml";
            if (!System.IO.File.Exists(templatePath))
            {
                templatePath = null;
            }
            string template = null;
            if (templatePath != null)
            {
                // get the file contents 
                template = System.IO.File.ReadAllText(templatePath);
            }
            else
            {
                template = EmbeddedFiles.ReadAllText("~/Views/" + folder + "/" + templateName + ".cshtml");
            }

            // pull out any instance of @TemplateData["XXX"]
            Regex regex = new Regex(@"@ViewData\[\""(.*?)\""\]");
            List<string> metas = new List<string>();
            var matches = regex.Matches(template);
            foreach (Match mtch in matches)
            {
                var meta = mtch.Value.Replace("@ViewData[\"", "").Replace("\"]", "");
                if (meta.StartsWith("Template."))
                    metas.Add(meta);
            }

            regex = new Regex(@"@Html.Raw\(ViewData\[\""(.*?)\""\]\)");
            matches = regex.Matches(template);
            foreach (Match mtch in matches)
            {
                var meta = mtch.Value.Replace("@Html.Raw(ViewData[\"", "").Replace("\"])", "");
                if (meta.StartsWith("Template."))
                    metas.Add(meta);
            }
            // return list of all XXX metas.
            return metas.Distinct().ToList();
        }
        public bool CheckSlug(string slug, int? id = null)
        {
            if (id.HasValue)
                return _db.Content.SingleOrDefault(c => c.Slug == slug && c.Id != id) == null;
            return _db.Content.SingleOrDefault(c => c.Slug == slug) == null;
        }
    }
}
