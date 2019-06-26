using Hood.Caching;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.IO;
using Hood.Models;
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
        private readonly IHoodCache _cache;
        private readonly IEventsService _events;
        private readonly IHostingEnvironment _env;

        public ContentRepository(
            HoodDbContext db,
            IHoodCache cache,
            IHostingEnvironment env,
            IEventsService events)
        {
            _db = db;
            _cache = cache;
            _events = events;
            _env = env;
        }

        // Content CRUD
        public async Task<ContentModel> GetPagedContent(ContentModel model, bool publishedOnly = true)
        {
            var content = GetContent(model.Search, model.Order, model.Type, model.Category, model.Filter, model.AuthorName, publishedOnly);
            await model.ReloadAsync(content);
            return model;
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
                        orderedContent = content.OrderByDescending(n => n.Metadata.SingleOrDefault(m => m.Name == sortVal).BaseValue);
                    else
                        orderedContent = content.OrderBy(n => n.Metadata.SingleOrDefault(m => m.Name == sortVal).BaseValue);
                }
                else
                {
                    switch (sort)
                    {
                        case "title":
                            orderedContent = content.OrderBy(n => n.Title);
                            break;
                        case "date":
                            orderedContent = content.OrderBy(n => n.CreatedOn);
                            break;
                        case "publish":
                            orderedContent = content.OrderBy(n => n.PublishDate);
                            break;
                        case "views":
                            orderedContent = content.OrderBy(n => n.Views);
                            break;

                        case "title+desc":
                            orderedContent = content.OrderByDescending(n => n.Title);
                            break;
                        case "date+desc":
                            orderedContent = content.OrderByDescending(n => n.CreatedOn);
                            break;
                        case "publish+desc":
                            orderedContent = content.OrderByDescending(n => n.PublishDate);
                            break;
                        case "views+desc":
                            orderedContent = content.OrderByDescending(n => n.Views);
                            break;

                        default:
                            orderedContent = content.OrderByDescending(n => n.CreatedOn);
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
            if (!_cache.TryGetValue(cacheKey, out List<Content> content))
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
        public Content GetContentByID(int id, bool clearCache = false, bool track = true)
        {
            string cacheKey = typeof(Content).ToString() + ".Single." + id;
            if (!_cache.TryGetValue(cacheKey, out Content content) || clearCache)
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
                _events.TriggerContentChanged(this);
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
                _events.TriggerContentChanged(this);
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
                _events.TriggerContentChanged(this);
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
                _events.TriggerContentChanged(this);
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
                _events.TriggerContentChanged(this);
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
                _db.Media.Add(new MediaObject(media));
                _db.Content.Update(content);
                await _db.SaveChangesAsync();
                _events.TriggerContentChanged(this);
                return new OperationResult<Content>(content);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message) as OperationResult<Content>;
            }
        }
        public Content DuplicateContent(int id)
        {
            var clone = _db.Content
                                 .AsNoTracking()
                                 .SingleOrDefault(c => c.Id == id);
            if (clone == null)
                return null;

            clone.Id = 0;
            clone.Title += " - Copy";
            clone.Slug += "-copy";

            _db.Content.Add(clone);
            _db.SaveChanges();

            var copyObject = _db.Content
                                .AsNoTracking()
                                .Include(p => p.Categories).ThenInclude(c => c.Category)
                                .Include(p => p.Tags).ThenInclude(t => t.Tag)
                                .Include(p => p.Media)
                                .Include(p => p.Metadata)
                                .SingleOrDefault(c => c.Id == id);

            clone.Categories = new List<ContentCategoryJoin>();
            foreach (var c in copyObject.Categories)
            {
                clone.Categories.Add(new ContentCategoryJoin() { ContentId = clone.Id, CategoryId = c.CategoryId });
            }
            clone.Tags = new List<ContentTagJoin>();
            foreach (var c in copyObject.Tags)
            {
                clone.Tags.Add(new ContentTagJoin() { ContentId = clone.Id, TagId = c.TagId });
            }
            clone.Media = new List<ContentMedia>();
            foreach (var c in copyObject.Media)
            {
                var newMedia = new ContentMedia();
                c.CopyProperties(newMedia);
                newMedia.Id = 0;
                clone.Media.Add(newMedia);
            }
            clone.Metadata = new List<ContentMeta>();
            foreach (var c in copyObject.Metadata)
            {
                var newMeta = new ContentMeta();
                c.CopyProperties(newMeta);
                newMeta.Id = 0;
                clone.Metadata.Add(newMeta);
            }
            _db.SaveChanges();

            return clone;
        }

        // Content Views
        public List<Content> GetRecent(string type, string categorySlug = null)
        {
            string cacheKey = typeof(Content).ToString() + ".Recent." + type;
            if (categorySlug.IsSet())
                cacheKey += "-" + categorySlug;
            if (!_cache.TryGetValue(cacheKey, out List<Content> content))
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
            if (!_cache.TryGetValue(cacheKey, out List<Content> content))
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
            if (!_cache.TryGetValue(cacheKey, out ContentNeighbours neighbours))
            {
                // get all the content - sorted by publish date.
                // find the ones either side of the id.
                var all = _db.Content
                         .Include(u => u.Metadata).Where(c => c.Status == (int)Status.Published && c.ContentType == type).AsNoTracking().ToArray();
                var index = Array.FindIndex(all, row => row.Id == id);
                neighbours = new ContentNeighbours()
                {
                    Next = all.ElementAtOrDefault(index + 1),
                    Previous = all.ElementAtOrDefault(index - 1)
                };
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
                _events.TriggerContentChanged(this);
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
                _events.TriggerContentChanged(this);
            }
            return new OperationResult(true);

        }
        #endregion

        #region "Categories" 
        public async Task<ContentCategory> GetCategoryById(int categoryId)
        {
            var category = await _db.ContentCategories.FirstOrDefaultAsync(c => c.Id == categoryId);
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
                _events.TriggerContentChanged(this);
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

            _db.ContentCategories.Add(category);
            await _db.SaveChangesAsync();
            _events.TriggerContentChanged(this);
            return new OperationResult<ContentCategory>(category);
        }
        public async Task<OperationResult> DeleteCategory(int categoryId)
        {
            var category = await _db.ContentCategories.FirstOrDefaultAsync(c => c.Id == categoryId);
            _db.Entry(category).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            _events.TriggerContentChanged(this);
            return new OperationResult(true);
        }
        public async Task<OperationResult> UpdateCategory(ContentCategory category)
        {
            _db.Update(category);
            await _db.SaveChangesAsync();
            _events.TriggerContentChanged(this);
            return new OperationResult(true);
        }
        public async Task<OperationResult> AddCategoryToContent(int contentId, int categoryId)
        {
            Content content = GetContentByID(contentId, true);

            if (content.IsInCategory(categoryId)) // Content is already in!
                return new OperationResult(true);

            var category = await _db.ContentCategories.SingleOrDefaultAsync(c => c.Id == categoryId);
            if (category == null)
                throw new Exception("The category does not exist.");

            content.Categories.Add(new ContentCategoryJoin() { CategoryId = category.Id, ContentId = content.Id });

            return Update(content);

        }
        public OperationResult RemoveCategoryFromContent(int contentId, int categoryId)
        {
            Content content = GetContentByID(contentId, true);

            if (!content.IsInCategory(categoryId))// Content is already out!
                return new OperationResult(true);

            var cat = content.Categories.SingleOrDefault(c => c.CategoryId == categoryId);

            if (cat == null)// Content is already out!
                return new OperationResult(true);

            content.Categories.Remove(cat);

            return Update(content);

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
            List<SitemapNode> nodes = new List<SitemapNode>
            {
                new SitemapNode()
                {
                    Url = urlHelper.AbsoluteUrl(""),
                    Priority = 1,
                    Frequency = SitemapFrequency.Daily
                },
                new SitemapNode()
                {
                    Url = urlHelper.AbsoluteUrl("contact"),
                    Priority = 0.9,
                    Frequency = SitemapFrequency.Never
                }
            };
            foreach (ContentType type in Engine.Settings.Content.AllowedTypes)
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
                        nodes.Add(new SitemapNode()
                        {
                            Url = urlHelper.AbsoluteUrl(content.Url.TrimStart('/')),
                            LastModified = content.PublishDate,
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
        public async Task<List<LinqToTwitter.Status>> GetTweets(string name, int count = 6)
        {
            string cacheKey = typeof(LinqToTwitter.Status).ToString() + ".Recent." + name.ToSeoUrl();
            List<LinqToTwitter.Status> tweets = new List<LinqToTwitter.Status>();
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

                //do
                //{
                //    // now add sinceID and maxID
                //    userStatusResponse =
                //        (from tweet in twitterCtx.Status
                //         where tweet.Type == LinqToTwitter.StatusType.User &&
                //               tweet.ScreenName == name &&
                //               tweet.Count == count &&
                //               tweet.SinceID == sinceID &&
                //               tweet.MaxID == maxID
                //         select tweet)
                //        .ToList();

                //    if (userStatusResponse.Count > 0)
                //    {
                //        // first tweet processed on current query
                //        maxID = userStatusResponse.Min(status => status.StatusID) - 1;

                //        statusList.AddRange(userStatusResponse);
                //    }
                //}
                //while (userStatusResponse.Count != 0 && statusList.Count < 30);
                tweets = statusList.Take(count).ToList();
                _cache.Add(cacheKey, tweets, new MemoryCacheEntryOptions() { AbsoluteExpiration = DateTime.Now.AddMinutes(1) });
                return tweets;
            }
        }
        public List<Country> AllCountries()
        {
            return Country.AllCountries;
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
            var type = Engine.Settings.Content.GetContentType(content.ContentType);
            if (type == null)
                return;
            foreach (CustomField field in type.CustomFields)
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
            _events.TriggerContentChanged(this);
        }
        public void RefreshAllMetas()
        {
            foreach (var content in _db.Content.Include(p => p.Metadata).ToList())
            {
                var type = Engine.Settings.Content.GetContentType(content.ContentType);
                if (type != null)
                {
                    RefreshMetas(content);
                    var currentTemplate = content.GetMeta("Settings.Template");
                    if (currentTemplate.GetStringValue().IsSet())
                    {
                        var template = currentTemplate.GetStringValue();
                        if (template.IsSet())
                        {
                            // delete all template metas that do not exist in the new template, and add any that are missing
                            List<string> newMetas = GetMetasForTemplate(template, type.TemplateFolder);
                            if (newMetas != null)
                                UpdateTemplateMetas(content, newMetas);
                        }
                    }
                }
            }
            _db.SaveChanges();
        }
        public List<string> GetMetasForTemplate(string templateName, string folder)
        {
            templateName = templateName.Replace("Meta:", "");

            // get the right template file (from theme or if it doesnt appear there from base)
            string templatePath = _env.ContentRootPath + "\\Themes\\" + Engine.Settings["Hood.Settings.Theme"] + "\\Views\\" + folder + "\\" + templateName + ".cshtml";
            if (!System.IO.File.Exists(templatePath))
                templatePath = _env.ContentRootPath + "\\Views\\" + folder + "\\" + templateName + ".cshtml";
            if (!System.IO.File.Exists(templatePath))
            {
                templatePath = null;
            }
            string template;
            if (templatePath != null)
            {
                // get the file contents 
                template = System.IO.File.ReadAllText(templatePath);
            }
            else
            {
                var path = "~/UI/" + folder + "/" + templateName + ".cshtml";
                if (EmbeddedFiles.GetFiles(path).Length > 0)
                    template = EmbeddedFiles.ReadAllText(path);
                else
                    return null;
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


        public object GetStatistics()
        {
            var totalPosts = _db.Content.Count();
            var totalPublished = _db.Content.Where(c => c.Status == (int)Status.Published && c.PublishDate < DateTime.Now).Count();
            var data = _db.Content.Where(c => c.Status == (int)Status.Published && c.PublishDate < DateTime.Now).Select(c => new { type = c.ContentType, date = c.CreatedOn.Date, month = c.CreatedOn.Month, pubdate = c.PublishDate.Date, pubmonth = c.PublishDate.Month }).ToList();

            var createdByDate = data.GroupBy(p => p.date).Select(g => new { name = g.Key, count = g.Count() });
            var createdByMonth = data.GroupBy(p => p.month).Select(g => new { name = g.Key, count = g.Count() });
            var publishedByDate = data.GroupBy(p => p.pubdate).Select(g => new { name = g.Key, count = g.Count() });
            var publishedByMonth = data.GroupBy(p => p.pubmonth).Select(g => new { name = g.Key, count = g.Count() });
            var byType = data.GroupBy(p => p.type).Select(g => new { type = Engine.Settings.Content.GetContentType(g.Key), total = g.Count(), typeName = g.Key });
            
            var days = new List<KeyValuePair<string, int>>();
            var publishDays = new List<KeyValuePair<string, int>>();
            foreach (DateTime day in DateTimeExtensions.EachDay(DateTime.Now.AddDays(-89), DateTime.Now))
            {
                var dayvalue = createdByDate.SingleOrDefault(c => c.name == day.Date);
                var count = dayvalue != null ? dayvalue.count : 0;
                days.Add(new KeyValuePair<string, int>(day.ToString("dd MMM"), count));

                dayvalue = publishedByDate.SingleOrDefault(c => c.name == day.Date);
                count = dayvalue != null ? dayvalue.count : 0;
                publishDays.Add(new KeyValuePair<string, int>(day.ToString("dd MMM"), count));
            }

            var months = new List<KeyValuePair<string, int>>();
            var publishMonths = new List<KeyValuePair<string, int>>();
            for (DateTime dt = DateTime.Now.AddMonths(-11); dt <= DateTime.Now; dt = dt.AddMonths(1))
            {
                var monthvalue = createdByMonth.SingleOrDefault(c => c.name == dt.Month);
                var count = monthvalue != null ? monthvalue.count : 0;
                months.Add(new KeyValuePair<string, int>(dt.ToString("MMMM, yyyy"), count));

                monthvalue = publishedByMonth.SingleOrDefault(c => c.name == dt.Month);
                count = monthvalue != null ? monthvalue.count : 0;
                publishMonths.Add(new KeyValuePair<string, int>(dt.ToString("MMMM, yyyy"), count));
            }

            return new { totalPosts, totalPublished, days, months, publishDays, publishMonths, byType };

        }
    }
}
