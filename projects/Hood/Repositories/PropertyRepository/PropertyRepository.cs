using Hood.Caching;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly HoodDbContext _db;
        private readonly IHoodCache _cache;
        private readonly IMediaManager _media;

        public PropertyRepository(
            HoodDbContext db,
            IHoodCache cache,
            IMediaManager media)
        {
            _db = db;
            _cache = cache;
            _media = media;
        }

        #region Property CRUD
        public async Task<PropertyListModel> GetPropertiesAsync(PropertyListModel model)
        {
            IQueryable<PropertyListing> properties = _db.Properties
                .Include(p => p.Media)
                .Include(p => p.FloorPlans)
                .Include(p => p.Agent)
                .Include(p => p.Metadata);

            // published?
            if (model.PublishStatus.HasValue)
                properties = properties.Where(p => p.Status == model.PublishStatus);

            if (!string.IsNullOrEmpty(model.Transaction))
            {
                if (model.Transaction == "Student")
                {
                    properties = properties.Where(n => n.ListingType == "Student");
                }
                else if (model.Transaction == "Sale")
                {
                    properties = properties.Where(n => n.ListingType == "Sale");
                }
                else
                {
                    properties = properties.Where(n => n.ListingType != "Sale" && n.ListingType != "Student");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(model.Type))
                    properties = properties.Where(n => n.ListingType == model.Type);
            }

            if (!string.IsNullOrEmpty(model.Agent))
                properties = properties.Where(n => n.Agent.UserName == model.Agent);

            if (!string.IsNullOrEmpty(model.PlanningType))
                properties = properties.Where(n => n.Planning == model.PlanningType);

            if (!string.IsNullOrEmpty(model.Status))
                properties = properties.Where(n => n.LeaseStatus == model.Status);

            if (model.Bedrooms.HasValue)
            {
                if (model.MaxBedrooms.HasValue)
                {
                    if (model.Bedrooms != -1)
                        properties = properties.Where(n => n.Bedrooms >= model.Bedrooms.Value);
                    if (model.MaxBedrooms != -1)
                        properties = properties.Where(n => n.Bedrooms <= model.MaxBedrooms.Value);
                }
                else
                    properties = properties.Where(n => n.Bedrooms == model.Bedrooms.Value);
            }

            if (model.MinRent.HasValue)
                properties = properties.Where(n => n.Rent >= model.MinRent.Value);
            if (model.MaxRent.HasValue)
                properties = properties.Where(n => n.Rent <= model.MaxRent.Value);

            if (model.MinPremium.HasValue)
                properties = properties.Where(n => n.Premium >= model.MinPremium.Value);
            if (model.MaxPremium.HasValue)
                properties = properties.Where(n => n.Premium <= model.MaxPremium.Value);

            if (model.MinPrice.HasValue)
                properties = properties.Where(n => n.AskingPrice >= model.MinPrice.Value);
            if (model.MaxPrice.HasValue)
                properties = properties.Where(n => n.AskingPrice <= model.MaxPrice.Value);

            // search the collection
            if (!string.IsNullOrEmpty(model.Search))
            {

                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                properties = properties.Where(n => searchTerms.Any(s => n.Title != null && n.Title.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Address1 != null && n.Address1.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Address2 != null && n.Address2.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.City != null && n.City.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.County != null && n.County.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Country != null && n.Country.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Postcode != null && n.Postcode.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.ShortDescription != null && n.ShortDescription.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Lease != null && n.Lease.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Location != null && n.Location.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Planning != null && n.Planning.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Reference != null && n.Reference.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
            }

            // sort the collection and then output it.
            if (!string.IsNullOrEmpty(model.Order))
            {
                switch (model.Order)
                {
                    case "name":
                    case "title":
                        properties = properties.OrderBy(n => n.Title);
                        break;
                    case "date":
                        properties = properties.OrderBy(n => n.PublishDate);
                        break;
                    case "views":
                        properties = properties.OrderBy(n => n.Views);
                        break;

                    case "name+desc":
                    case "title+desc":
                        properties = properties.OrderByDescending(n => n.Title);
                        break;
                    case "date+desc":
                        properties = properties.OrderByDescending(n => n.PublishDate);
                        break;
                    case "views+desc":
                        properties = properties.OrderByDescending(n => n.Views);
                        break;

                    case "rent":
                        properties = properties.OrderBy(n => n.Rent);
                        break;
                    case "planning":
                        properties = properties.OrderBy(n => n.Planning);
                        break;
                    case "type":
                        properties = properties.OrderBy(n => n.ListingType);
                        break;
                    case "premium":
                        properties = properties.OrderBy(n => n.Premium);
                        break;

                    case "rent+desc":
                        properties = properties.OrderByDescending(n => n.Rent);
                        break;
                    case "planning+desc":
                        properties = properties.OrderByDescending(n => n.Planning);
                        break;
                    case "type+desc":
                        properties = properties.OrderByDescending(n => n.ListingType);
                        break;
                    case "premium+desc":
                        properties = properties.OrderByDescending(n => n.Premium);
                        break;

                    default:
                        properties = properties.OrderByDescending(n => n.PublishDate).ThenBy(n => n.Id);
                        break;
                }
            }

            await model.ReloadAsync(properties);
            return model;
        }
        public async Task<List<MapMarker>> GetLocationsAsync(PropertyListModel filters)
        {
            var propertiesQuery = await GetPropertiesAsync(filters);
            return propertiesQuery.List.Select(p =>
                new MapMarker(p, p.Title, p.QuickInfo, p.Id.ToString(), p.Url, p.FeaturedImage.Url)
            ).ToList();
        }
        public async Task<PropertyListModel> GetFeaturedAsync()
        {
            string cacheKey = typeof(PropertyListModel) + ".Featured";
            if (_cache.TryGetValue(cacheKey, out PropertyListModel properties))
                return properties;
            else
            {
                properties = await GetPropertiesAsync(new PropertyListModel() { Featured = true, PageSize = int.MaxValue });
                _cache.Add(cacheKey, properties, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
                return properties;
            }
        }
        public async Task<PropertyListModel> GetRecentAsync()
        {
            string cacheKey = typeof(PropertyListModel) + ".Recent";
            if (_cache.TryGetValue(cacheKey, out PropertyListModel properties))
                return properties;
            else
            {
                properties = await GetPropertiesAsync(new PropertyListModel() { PageSize = int.MaxValue, Order = "DateDesc" });
                _cache.Add(cacheKey, properties, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
                return properties;
            }
        }
        public async Task<PropertyListing> GetPropertyByIdAsync(int id, bool nocache = false)
        {
            string cacheKey = typeof(PropertyListing) + ".Single." + id.ToString();
            if (_cache.TryGetValue(cacheKey, out PropertyListing property) && !nocache)
                return property;
            else
            {
                property = await _db.Properties
                    .Include(p => p.Media)
                    .Include(p => p.FloorPlans)
                    .Include(p => p.Agent)
                    .Include(p => p.Metadata)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                _cache.Add(cacheKey, property, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
                return property;
            }
        }
        public async Task<PropertyListing> AddAsync(PropertyListing property)
        {
            _db.Properties.Add(property);
            await _db.SaveChangesAsync();
            return property;
        }
        public async Task UpdateAsync(PropertyListing property)
        {
            _db.Update(property);
            await _db.SaveChangesAsync();
            _db.Entry(property).State = EntityState.Detached;
            ClearPropertyCache(property.Id);
        }
        public async Task SetStatusAsync(int id, ContentStatus status)
        {
            PropertyListing property = _db.Properties.Where(p => p.Id == id).FirstOrDefault();
            property.Status = status;
            await _db.SaveChangesAsync();
            ClearPropertyCache(property.Id);
        }
        public async Task DeleteAsync(int id)
        {
            PropertyListing property = await _db.Properties
                .Include(p => p.Metadata)
                .Include(p => p.Media)
                .Include(p => p.FloorPlans)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            property.Media.ForEach(async m =>
            {
                try { await _media.DeleteStoredMedia(m); } catch (Exception) { }
                _db.Entry(m).State = EntityState.Deleted;

            });
            property.FloorPlans.ForEach(async m =>
            {
                try { await _media.DeleteStoredMedia(m); } catch (Exception) { }
                _db.Entry(m).State = EntityState.Deleted;

            });
            property.Metadata.ForEach(m =>
            {
                _db.Entry(m).State = EntityState.Deleted;
            });
            await _db.SaveChangesAsync();

            _db.Entry(property).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            ClearPropertyCache(id);
        }
        public async Task DeleteAllAsync()
        {
            var all = _db.Properties
                .Include(p => p.Metadata)
                .Include(p => p.Media)
                .Include(p => p.FloorPlans);

            all.ForEach(p =>
            {
                p.Media.ForEach(async m =>
                {
                    try { await _media.DeleteStoredMedia(m); } catch (Exception) { }
                    _db.Entry(m).State = EntityState.Deleted;

                });
                p.FloorPlans.ForEach(async m =>
                {
                    try { await _media.DeleteStoredMedia(m); } catch (Exception) { }
                    _db.Entry(m).State = EntityState.Deleted;

                });
                p.Metadata.ForEach(m =>
                {
                    _db.Entry(m).State = EntityState.Deleted;
                });
            });
            await _db.SaveChangesAsync();
            all.ForEach(p =>
            {
                _db.Entry(p).State = EntityState.Deleted;
            });
            await _db.SaveChangesAsync();
            ClearPropertyCache();
        }
        public async Task<MediaDirectory> GetDirectoryAsync()
        {
            MediaDirectory contentDirectory = await _db.MediaDirectories.SingleOrDefaultAsync(md => md.Slug == MediaManager.PropertyDirectorySlug && md.Type == DirectoryType.System);
            if (contentDirectory == null)
            {
                throw new Exception("Site folder is not available.");
            }
            return contentDirectory;
        }
        #endregion

        #region Direct Field Editing
        public async Task ClearFieldAsync(int id, string field)
        {
            PropertyListing property = _db.Properties.FirstOrDefault(c => c.Id == id);
            _db.Entry(property).Property(field).CurrentValue = null;
            await _db.SaveChangesAsync();
            ClearPropertyCache(id);
        }
        #endregion

        #region Media / Floor Plans
        public async Task AddMediaAsync(PropertyListing property, PropertyMedia media)
        {
            if (property.Media == null)
                property.Media = new List<PropertyMedia>();
            property.Media.Add(media);
            _db.Properties.Update(property);
            await _db.SaveChangesAsync();
            ClearPropertyCache(property.Id);
        }
        public async Task AddFloorplanAsync(PropertyListing property, PropertyFloorplan media)
        {
            if (property.FloorPlans == null)
                property.FloorPlans = new List<PropertyFloorplan>();
            property.FloorPlans.Add(media);
            _db.Properties.Update(property);
            await _db.SaveChangesAsync();
            ClearPropertyCache(property.Id);
        }
        public async Task<PropertyListing> RemoveMediaAsync(int id, int mediaId)
        {
            var property = await _db.Properties.Include(p => p.Media).SingleOrDefaultAsync(p => p.Id == id);
            var media = property.Media.SingleOrDefault(m => m.Id == mediaId);
            if (media != null)
                property.Media.Remove(media);
            await _db.SaveChangesAsync();
            return property;
        }
        public async Task<PropertyListing> RemoveFloorplanAsync(int id, int mediaId)
        {
            var property = await _db.Properties.Include(p => p.FloorPlans).SingleOrDefaultAsync(p => p.Id == id);
            var media = property.FloorPlans.SingleOrDefault(m => m.Id == mediaId);
            if (media != null)
                property.FloorPlans.Remove(media);
            await _db.SaveChangesAsync();
            return property;
        }
        #endregion

        #region Cache
        private void ClearPropertyCache(int? id = null)
        {
            _cache.Remove("Property:Featured");
            _cache.Remove("Property:Recent");
            if (id.HasValue)
                _cache.Remove("Property:Listing:" + id.ToString());
        }
        #endregion

        #region Statistics
        public async Task<object> GetStatisticsAsync()
        {
            var totalPosts = await _db.Properties.CountAsync();
            var totalPublished = await _db.Properties.Where(c => c.Status == ContentStatus.Published && c.PublishDate < DateTime.Now).CountAsync();
            var data = await _db.Properties.Select(c => new { date = c.CreatedOn.Date, month = c.CreatedOn.Month, pubdate = c.PublishDate.Date, pubmonth = c.PublishDate.Month }).ToListAsync();

            var createdByDate = data.GroupBy(p => p.date).Select(g => new { name = g.Key, count = g.Count() });
            var createdByMonth = data.GroupBy(p => p.month).Select(g => new { name = g.Key, count = g.Count() });
            var publishedByDate = data.GroupBy(p => p.pubdate).Select(g => new { name = g.Key, count = g.Count() });
            var publishedByMonth = data.GroupBy(p => p.pubmonth).Select(g => new { name = g.Key, count = g.Count() });

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

            return new { totalPosts, totalPublished, days, months, publishDays, publishMonths };
        }
        #endregion
    }
}
