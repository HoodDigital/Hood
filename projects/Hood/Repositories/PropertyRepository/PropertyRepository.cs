using Hood.Caching;
using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Models;
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
        private readonly IConfiguration _config;
        private readonly ISettingsRepository _settings;
        private readonly IHoodCache _cache;

        public PropertyRepository(HoodDbContext db, IHoodCache cache, IConfiguration config, ISettingsRepository site)
        {
            _db = db;
            _config = config;
            _settings = site;
            _cache = cache;
        }

        public OperationResult<PropertyListing> Add(PropertyListing property)
        {
            try
            {


                _db.Properties.Add(property);
                _db.SaveChanges();
                var result = new OperationResult<PropertyListing>(property);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message) as OperationResult<PropertyListing>;
            }
        }
        public OperationResult Delete(int id)
        {
            try
            {
                PropertyListing property = _db.Properties.Include(p => p.Media).Where(p => p.Id == id).FirstOrDefault();
                _db.Entry(property).State = EntityState.Deleted;
                _db.SaveChanges();
                ClearPropertyCache(id);
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message);
            }
        }
        public PropertyListing GetPropertyById(int id, bool nocache = false)
        {
            string cacheKey = typeof(PropertyListing) + ".Single." + id.ToString();
            if (_cache.TryGetValue(cacheKey, out PropertyListing property) && !nocache)
                return property;
            else
            {
                property = _db.Properties
                    .Include(p => p.Media)
                    .Include(p => p.FloorPlans)
                    .Include(p => p.Agent)
                    .Include(p => p.Metadata)
                    .AsNoTracking()
                    .FirstOrDefault(c => c.Id == id);

                _cache.Add(cacheKey, property, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
                return property;
            }
        }
        private IQueryable<PropertyListing> GetProperties(PropertySearchModel propertyFilters, bool published)
        {
            IQueryable<PropertyListing> properties = _db.Properties
                .Include(p => p.Media)
                .Include(p => p.FloorPlans)
                .Include(p => p.Agent)
                .Include(p => p.Metadata);

            // published?
            if (published)
                properties = properties.Where(p => p.Status == (int)Status.Published);

            if (!string.IsNullOrEmpty(propertyFilters.Transaction))
            {
                if (propertyFilters.Transaction == "Student")
                {
                    properties = properties.Where(n => n.ListingType == "Student");
                }
                else if (propertyFilters.Transaction == "Sale")
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
                if (!string.IsNullOrEmpty(propertyFilters.Type))
                    properties = properties.Where(n => n.ListingType == propertyFilters.Type);
            }

            if (!string.IsNullOrEmpty(propertyFilters.Agent))
                properties = properties.Where(n => n.Agent.UserName == propertyFilters.Agent);

            if (!string.IsNullOrEmpty(propertyFilters.PlanningType))
                properties = properties.Where(n => n.Planning == propertyFilters.PlanningType);

            if (!string.IsNullOrEmpty(propertyFilters.Status))
                properties = properties.Where(n => n.LeaseStatus == propertyFilters.Status);

            if (propertyFilters.Bedrooms.HasValue)
                properties = properties.Where(n => n.Bedrooms == propertyFilters.Bedrooms.Value);

            if (propertyFilters.MinRent.HasValue)
                properties = properties.Where(n => n.Rent >= propertyFilters.MinRent.Value);
            if (propertyFilters.MaxRent.HasValue)
                properties = properties.Where(n => n.Rent <= propertyFilters.MaxRent.Value);

            if (propertyFilters.MinPremium.HasValue)
                properties = properties.Where(n => n.Premium >= propertyFilters.MinPremium.Value);
            if (propertyFilters.MaxPremium.HasValue)
                properties = properties.Where(n => n.Premium <= propertyFilters.MaxPremium.Value);

            if (propertyFilters.MinPrice.HasValue)
                properties = properties.Where(n => n.AskingPrice >= propertyFilters.MinPrice.Value);
            if (propertyFilters.MaxPrice.HasValue)
                properties = properties.Where(n => n.AskingPrice <= propertyFilters.MaxPrice.Value);

            // search the collection
            if (!string.IsNullOrEmpty(propertyFilters.Search))
            {

                string[] searchTerms = propertyFilters.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
            if (!string.IsNullOrEmpty(propertyFilters.Order))
            {
                switch (propertyFilters.Order)
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
            return properties;
        }
        public async Task<PropertySearchModel> GetPagedProperties(PropertySearchModel model, bool published = true)
        {
            var propertiesQuery = GetProperties(model, published);
            await model.ReloadAsync(propertiesQuery);
            return model;
        }
        public Task<List<MapMarker>> GetLocations(PropertySearchModel filters)
        {
            var propertiesQuery = GetProperties(filters, true);
            return propertiesQuery.AsNoTracking().Select(p =>
                new MapMarker(p, p.Title, p.Id.ToString(), p.QuickInfo, p.Url, p.FeaturedImage.Url)
            ).ToListAsync();


        }
        public OperationResult<PropertyListing> UpdateProperty(PropertyListing property)
        {
            try
            {
                var changes = _db.Update(property);
                _db.SaveChanges();
                _db.Entry(property).State = EntityState.Detached;
                ClearPropertyCache(property.Id);
                return new OperationResult<PropertyListing>(property);
            }
            catch (DbUpdateException ex)
            {
                return new OperationResult(ex) as OperationResult<PropertyListing>;
            }
        }
        public void ClearField(int id, string field)
        {
            PropertyListing property = _db.Properties.FirstOrDefault(c => c.Id == id);
            _db.Entry(property).Property(field).CurrentValue = null;
            _db.SaveChanges();
            ClearPropertyCache(id);
        }
        public OperationResult<PropertyListing> SetStatus(int id, Status status)
        {
            try
            {
                PropertyListing property = _db.Properties.Where(p => p.Id == id).FirstOrDefault();
                property.Status = (int)status;
                _db.SaveChanges();
                ClearPropertyCache(property.Id);
                return new OperationResult<PropertyListing>(property);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message) as OperationResult<PropertyListing>;
            }
        }
        public async Task<OperationResult> DeleteAll()
        {
            try
            {
                _db.Properties.ForEach(p =>
                {
                    _db.Entry(p).State = EntityState.Deleted;
                });
                await _db.SaveChangesAsync();
                ClearPropertyCache();
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message);
            }
        }
        public async Task<OperationResult<PropertyListing>> AddImage(PropertyListing property, PropertyMedia media)
        {
            try
            {
                if (property.Media == null)
                    property.Media = new List<PropertyMedia>();
                property.Media.Add(media);
                _db.Media.Add(new MediaObject(media));
                _db.Properties.Update(property);
                await _db.SaveChangesAsync();
                ClearPropertyCache(property.Id);
                return new OperationResult<PropertyListing>(property);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message) as OperationResult<PropertyListing>;
            }
        }
        public async Task<OperationResult<PropertyListing>> AddFloorplan(PropertyListing property, PropertyFloorplan media)
        {
            try
            {
                if (property.FloorPlans == null)
                    property.FloorPlans = new List<PropertyFloorplan>();
                property.FloorPlans.Add(media);
                _db.Media.Add(new MediaObject(media));
                _db.Properties.Update(property);
                await _db.SaveChangesAsync();
                ClearPropertyCache(property.Id);
                return new OperationResult<PropertyListing>(property);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message) as OperationResult<PropertyListing>;
            }
        }
        public async Task<List<PropertyListing>> GetFeatured()
        {
            string cacheKey = typeof(PropertyListing) + ".Featured";
            if (_cache.TryGetValue(cacheKey, out List<PropertyListing> properties))
                return properties;
            else
            {
                properties = await _db.Properties
                    .Include(p => p.Media)
                    .Include(p => p.FloorPlans)
                    .Include(p => p.Agent)
                    .Include(p => p.Metadata)
                    .Where(c => c.Featured && c.Status == (int)Status.Published)
                    .Take(20)
                    .ToListAsync();
                _cache.Add(cacheKey, properties, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
                return properties;
            }
        }
        public async Task<List<PropertyListing>> GetRecent()
        {
            string cacheKey = typeof(PropertyListing) + ".Recent";
            if (_cache.TryGetValue(cacheKey, out List<PropertyListing> properties))
                return properties;
            else
            {
                properties = await _db.Properties
                    .Include(p => p.Media)
                    .Include(p => p.FloorPlans)
                    .Include(p => p.Agent)
                    .Include(p => p.Metadata)
                    .Where(c => c.Status == (int)Status.Published)
                    .OrderByDescending(p => p.LastEditedOn)
                    .ThenByDescending(p => p.CreatedOn)
                    .Take(20)
                    .ToListAsync();
                _cache.Add(cacheKey, properties, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
                return properties;
            }
        }
        private void ClearPropertyCache(int? id = null)
        {
            _cache.Remove("Property:Featured");
            _cache.Remove("Property:Recent");
            if (id.HasValue)
                _cache.Remove("Property:Listing:" + id.ToString());
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

        public object GetStatistics()
        {
            var totalPosts = _db.Properties.Count();
            var totalPublished = _db.Properties.Where(c => c.Status == (int)Status.Published && c.PublishDate < DateTime.Now).Count();
            var data = _db.Properties.Select(c => new { date = c.CreatedOn.Date, month = c.CreatedOn.Month, pubdate = c.PublishDate.Date, pubmonth = c.PublishDate.Month }).ToList();

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

    }
}
