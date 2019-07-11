using Hood.Caching;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.IO;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IEventsService _eventService;
        private readonly IHostingEnvironment _env;

        public ContentRepository(
            HoodDbContext db,
            IHoodCache cache,
            IHostingEnvironment env,
            IEventsService eventService)
        {
            _db = db;
            _cache = cache;
            _eventService = eventService;
            _env = env;
        }

        #region Content CRUD
        public async Task<ContentModel> GetContentAsync(ContentModel model)
        {
            IQueryable<Content> content = _db.Content.Include(p => p.Author)
                                                     .Include(p => p.Media)
                                                     .Include(p => p.Metadata)
                                                     .Include(p => p.Categories).ThenInclude(c => c.Category)
                                                     .Include(p => p.Tags).ThenInclude(t => t.Tag).AsNoTracking();

            // filter posts by type
            if (!string.IsNullOrEmpty(model.Type))
            {
                content = content.Where(c => c.ContentType == model.Type);
            }

            // published?
            if (model.Status.HasValue)
            {
                content = content.Where(p => p.Status == model.Status.Value);
            }

            // posts by author
            if (!string.IsNullOrEmpty(model.AuthorName))
            {
                content = content.Where(n => n.Author.UserName == model.AuthorName);
            }

            if (model.Categories != null && model.Categories.Count > 0)
            {
                content = content.Where(c => c.Categories.Any(cc => model.Categories.Any(mc => cc.Category.Slug == mc)));
            }
            if (!string.IsNullOrEmpty(model.Category))
            {
                content = content.Where(c => c.Categories.Any(cc => cc.Category.Slug == model.Category));
            }

            // search the collection

            if (!string.IsNullOrEmpty(model.Search))
            {

                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                content = content.Where(n => searchTerms.Any(s => n.Title != null && n.Title.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Body != null && n.Body.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Excerpt != null && n.Excerpt.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Metadata.Any(m => m.BaseValue != null && m.BaseValue.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)));
            }

            if (!string.IsNullOrEmpty(model.Filter))
            {
                if (model.Filter.StartsWith("Meta:"))
                {
                    model.Filter = model.Filter.Replace("Meta:", "");
                    // search for a specific meta value 
                    if (model.Filter.Contains("="))
                    {
                        var meta = model.Filter.Split('=')[0];
                        var str = model.Filter.Split('=');
                        var value = str[str.Length - 1];
                        var jsonValue = JsonConvert.SerializeObject(value);
                        content = content.Where(c => c.Metadata.Any(cm => cm.Name == meta && cm.BaseValue == jsonValue));
                    }
                }
            }

            // sort the collection and then content it.
            if (!string.IsNullOrEmpty(model.Order))
            {
                if (model.Order.StartsWith("Meta"))
                {
                    var sortVal = model.Order.Replace("Meta:", "").Replace(":Desc", "");
                    if (model.Order.EndsWith("Desc"))
                        content = content.OrderByDescending(n => n.Metadata.SingleOrDefault(m => m.Name == sortVal).BaseValue);
                    else
                        content = content.OrderBy(n => n.Metadata.SingleOrDefault(m => m.Name == sortVal).BaseValue);
                }
                else
                {
                    switch (model.Order)
                    {
                        case "Title":
                            content = content.OrderBy(n => n.Title);
                            break;
                        case "Date":
                            content = content.OrderBy(n => n.CreatedOn);
                            break;
                        case "Publish":
                            content = content.OrderBy(n => n.PublishDate);
                            break;
                        case "Views":
                            content = content.OrderBy(n => n.Views);
                            break;

                        case "TitleDesc":
                            content = content.OrderByDescending(n => n.Title);
                            break;
                        case "DateDesc":
                            content = content.OrderByDescending(n => n.CreatedOn);
                            break;
                        case "PublishDesc":
                            content = content.OrderByDescending(n => n.PublishDate);
                            break;
                        case "ViewsDesc":
                            content = content.OrderByDescending(n => n.Views);
                            break;

                        default:
                            content = content.OrderByDescending(n => n.CreatedOn);
                            break;
                    }
                }
            }
            await model.ReloadAsync(content);
            return model;
        }
        public async Task<Content> GetContentByIdAsync(int id, bool clearCache = false, bool track = true)
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
                await RefreshMetasAsync(content);
                _cache.Add(cacheKey, content, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
            }
            return content;
        }
        public async Task<Content> AddAsync(Content content)
        {
            // create the slug
            var generator = new KeyGenerator();
            content.Slug = generator.UrlSlug();
            while (!await CheckSlugAsync(content.Slug))
                content.Slug = generator.UrlSlug();

            _db.Content.Add(content);
            await _db.SaveChangesAsync();
            content = await GetContentByIdAsync(content.Id);
            await RefreshMetasAsync(content);
            _eventService.TriggerContentChanged(this);
            return content;
        }
        public async Task<Content> UpdateAsync(Content content)
        {
            string cacheKey = typeof(Content).ToString() + ".Single." + content.Id;
            _db.Update(content);
            await _db.SaveChangesAsync();
            _eventService.TriggerContentChanged(this);
            _cache.Add(cacheKey, content, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
            return content;
        }
        public async Task DeleteAsync(int id)
        {
            Content content = _db.Content.Where(p => p.Id == id).FirstOrDefault();
            _db.Entry(content).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
        }
        public async Task SetStatusAsync(int id, ContentStatus status)
        {
            Content content = _db.Content.Where(p => p.Id == id).FirstOrDefault();
            content.Status = status;
            await UpdateAsync(content);
        }
        public async Task DeleteAllAsync(string type)
        {
            _db.Content.Where(c => c.ContentType == type).ForEach(p =>
            {
                _db.Entry(p).State = EntityState.Deleted;
                string cacheKey = typeof(Content).ToString() + ".Single." + p.Id;
                _cache.Remove(cacheKey);
            });
            await _db.SaveChangesAsync();
            _eventService.TriggerContentChanged(this);
        }
        #endregion

        #region Duplicate
        public async Task<Content> DuplicateContentAsync(int id)
        {
            var clone = await _db.Content
                                 .AsNoTracking()
                                 .SingleOrDefaultAsync(c => c.Id == id);
            if (clone == null)
                return null;

            clone.Id = 0;
            clone.Title += " - Copy";
            clone.Slug += "-copy";

            _db.Content.Add(clone);
            await _db.SaveChangesAsync();

            var copyObject = await _db.Content
                                .AsNoTracking()
                                .Include(p => p.Categories).ThenInclude(c => c.Category)
                                .Include(p => p.Tags).ThenInclude(t => t.Tag)
                                .Include(p => p.Media)
                                .Include(p => p.Metadata)
                                .SingleOrDefaultAsync(c => c.Id == id);

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
            await _db.SaveChangesAsync();

            return clone;
        }
        #endregion

        #region Images
        public async Task AddImageAsync(Content content, ContentMedia media)
        {
            if (content.Media == null)
                content.Media = new List<ContentMedia>();
            _db.Media.Add(new MediaObject(media));
            content.Media.Add(media);
            await UpdateAsync(content);
        }
        #endregion

        #region Content Views
        public async Task<ContentModel> GetRecentAsync(string type, string category = null)
        {
            string cacheKey = typeof(ContentModel).ToString() + ".Recent." + type;
            if (category.IsSet())
                cacheKey += "-" + category;
            if (!_cache.TryGetValue(cacheKey, out ContentModel content))
            {
                content = await GetContentAsync(new ContentModel() { Type = type, Category = category, PageSize = int.MaxValue, Order = "DateDesc" });
                _cache.Add(cacheKey, content, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return content;
        }
        public async Task<ContentModel> GetFeaturedAsync(string type, string category = null)
        {
            string cacheKey = typeof(ContentModel).ToString() + ".Featured." + type;
            if (category.IsSet())
                cacheKey += "." + category;
            if (!_cache.TryGetValue(cacheKey, out ContentModel content))
            {
                content = await GetContentAsync(new ContentModel() { Featured = true, Type = type, Category = category, PageSize = int.MaxValue });
                _cache.Add(cacheKey, content, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return content;
        }
        public async Task<ContentNeighbours> GetNeighbourContentAsync(int id, string type, string category = null)
        {
            string cacheKey = typeof(Content).ToString() + ".Neighbours." + id;
            if (!_cache.TryGetValue(cacheKey, out ContentNeighbours neighbours))
            {
                // get all the content - sorted by publish date.
                // find the ones either side of the id.
                var all = (await GetContentAsync(new ContentModel() { Status = ContentStatus.Published, Category = category, Type = type, PageSize = int.MaxValue, Order = "Date" })).List.ToArray();
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
        #endregion

        #region Tags
        public async Task<ContentTag> AddTagAsync(string value)
        {
            value = value.Trim().ToTitleCase();
            var tag = _db.ContentTags.SingleOrDefault(t => t.Value == value);
            if (tag == null)
            {
                tag = new ContentTag() { Value = value };
                _db.ContentTags.Add(tag);
                await _db.SaveChangesAsync();
                _eventService.TriggerContentChanged(this);
            }
            return tag;
        }
        public async Task DeleteTagAsync(string value)
        {
            value = value.ToTitleCase();
            var tag = _db.ContentTags.SingleOrDefault(t => t.Value == value);
            if (tag != null)
            {
                _db.ContentTags.Remove(tag);
                await _db.SaveChangesAsync();
                _eventService.TriggerContentChanged(this);
            }
        }
        #endregion

        #region Categories 
        public async Task<ContentCategory> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _db.ContentCategories.FirstOrDefaultAsync(c => c.Id == categoryId);
            return category;
        }
        public async Task<IEnumerable<ContentCategory>> GetCategoriesAsync(int contentId)
        {
            var club = await GetContentByIdAsync(contentId);
            return club?.Categories?.Select(c => c.Category);
        }
        public async Task<ContentCategory> AddCategoryAsync(string value, string type)
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
                _eventService.TriggerContentChanged(this);
            }
            return category;
        }
        public async Task<ContentCategory> AddCategoryAsync(ContentCategory category)
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
            _eventService.TriggerContentChanged(this);
            return category;
        }
        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _db.ContentCategories.FirstOrDefaultAsync(c => c.Id == categoryId);
            _db.Entry(category).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            _eventService.TriggerContentChanged(this);
        }
        public async Task UpdateCategoryAsync(ContentCategory category)
        {
            _db.Update(category);
            await _db.SaveChangesAsync();
            _eventService.TriggerContentChanged(this);
        }
        public async Task AddCategoryToContentAsync(int contentId, int categoryId)
        {
            Content content = await GetContentByIdAsync(contentId, true);

            if (content.IsInCategory(categoryId)) // Content is already in!
                return;

            var category = await _db.ContentCategories.SingleOrDefaultAsync(c => c.Id == categoryId);
            if (category == null)
                throw new Exception("The category does not exist.");

            content.Categories.Add(new ContentCategoryJoin() { CategoryId = category.Id, ContentId = content.Id });

            await UpdateAsync(content);
        }
        public async Task RemoveCategoryFromContentAsync(int contentId, int categoryId)
        {
            Content content = await GetContentByIdAsync(contentId, true);

            if (!content.IsInCategory(categoryId))// Content is already out!
                return;

            var cat = content.Categories.SingleOrDefault(c => c.CategoryId == categoryId);

            if (cat == null)// Content is already out!
                return;

            content.Categories.Remove(cat);

            await UpdateAsync(content);
        }
        #endregion

        #region Sitemap
        // Sitemap
        public List<Content> GetPages(string category = null)
        {
            string cacheKey = typeof(Content).ToString() + ".Pages";
            if (!_cache.TryGetValue(cacheKey, out List<Content> pages))
            {
                var content = GetContentAsync(new ContentModel() { Type = "page", PageSize = int.MaxValue, Category = category }).Result;
                pages = content.List;
                _cache.Add(cacheKey, pages, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
            }
            return pages;
        }
        public async Task<string> GetSitemapDocumentAsync(IUrlHelper urlHelper)
        {
            string cacheKey = typeof(Content).ToString() + ".SitemapDocument";
            if (!_cache.TryGetValue(cacheKey, out string pages))
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
                        var typeContent = await GetContentAsync(new ContentModel() { Type = type.Type, PageSize = int.MaxValue });
                        foreach (var content in typeContent.List.OrderByDescending(c => c.PublishDate))
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
                pages = document.ToString();
                _cache.Add(cacheKey, pages, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
            }
            return pages;
        }

        #endregion

        #region Metadata
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
        public async Task RefreshMetasAsync(Content content)
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
            await _db.SaveChangesAsync();
            _eventService.TriggerContentChanged(this);
        }
        public async Task RefreshAllMetasAsync()
        {
            foreach (var content in _db.Content.Include(p => p.Metadata).ToList())
            {
                var type = Engine.Settings.Content.GetContentType(content.ContentType);
                if (type != null)
                {
                    await RefreshMetasAsync(content);
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
            await _db.SaveChangesAsync();
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
        #endregion

        #region Helpers
        public async Task<bool> CheckSlugAsync(string slug, int? id = null)
        {
            if (id.HasValue)
                return await _db.Content.AnyAsync(c => c.Slug == slug && c.Id != id);
            return await _db.Content.AnyAsync(c => c.Slug == slug);
        }
        #endregion

        #region Statistics
        public async Task<object> GetStatisticsAsync()
        {
            var totalPosts = await _db.Content.CountAsync();
            var totalPublished = await _db.Content.Where(c => c.Status == ContentStatus.Published && c.PublishDate < DateTime.Now).CountAsync();
            var data = await _db.Content.Where(c => c.Status == ContentStatus.Published && c.PublishDate < DateTime.Now).Select(c => new { type = c.ContentType, date = c.CreatedOn.Date, month = c.CreatedOn.Month, pubdate = c.PublishDate.Date, pubmonth = c.PublishDate.Month }).ToListAsync();

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
        #endregion

        #region Obsoletes 
        [Obsolete("Use List<Country> Country.AllCountries from now on.", true)]
        public List<Country> AllCountries()
        {
            return Countries.All;
        }
        [Obsolete("Use static Country Country.GetCountry(string name) from now on.", true)]
        public Country GetCountry(string name)
        {
            return Countries.GetCountry(name);
        }
        #endregion

        #region LinqToTwitter
        [Obsolete("No longer supported, should be loaded in theme if required. Will be removed in next version.", true)]
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

                maxID = userStatusResponse.Min(status => status.StatusID) - 1;

                tweets = statusList.Take(count).ToList();
                _cache.Add(cacheKey, tweets, new MemoryCacheEntryOptions() { AbsoluteExpiration = DateTime.Now.AddMinutes(1) });
                return tweets;
            }
        }
        #endregion

    }
}
