using Hood.Caching;
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
            PropertyListing property = null;
            if (_cache.TryGetValue(cacheKey, out property) && !nocache)
                return property;
            else
            {
                property = _db.Properties
                    .Include(p => p.Media)
                    .Include(p => p.FloorPlans)
                    .Include(p => p.Agent)
                    .Include(p => p.Metadata)
                    .FirstOrDefault(c => c.Id == id);

                _cache.Add(cacheKey, property, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60)));
                return property;
            }
        }
        private IQueryable<PropertyListing> GetProperties(PropertyFilters propertyFilters, bool published)
        {
            IQueryable<PropertyListing> properties = _db.Properties
                .Include(p => p.Media)
                .Include(p => p.FloorPlans)
                .Include(p => p.Agent)
                .Include(p => p.Metadata);

            // published?
            if (published)
                properties = properties.Where(p => p.Status == (int)Status.Published);

            // posts by author
            if (!string.IsNullOrEmpty(propertyFilters.Agent))
                properties = properties.Where(n => n.Agent.UserName == propertyFilters.Agent);

            // posts by author
            if (!string.IsNullOrEmpty(propertyFilters.PlanningType))
                properties = properties.Where(n => n.Planning == propertyFilters.PlanningType);

            // posts by author
            if (!string.IsNullOrEmpty(propertyFilters.Type))
                properties = properties.Where(n => n.ListingType == propertyFilters.Type);

            if (propertyFilters.MinBedrooms.HasValue)
                properties = properties.Where(n => n.Bedrooms >= propertyFilters.MinBedrooms.Value);
            if (propertyFilters.MaxBedrooms.HasValue)
                properties = properties.Where(n => n.Bedrooms <= propertyFilters.MaxBedrooms.Value);

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
            if (!string.IsNullOrEmpty(propertyFilters.search))
            {

                string[] searchTerms = propertyFilters.search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                                      || searchTerms.Any(s => n.Reference != null && n.Reference.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                      || searchTerms.Any(s => n.Metadata.Any(m => m.BaseValue != null && m.BaseValue.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)));
            }

            // sort the collection and then output it.
            if (!string.IsNullOrEmpty(propertyFilters.sort))
            {
                switch (propertyFilters.sort)
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
        public async Task<PagedList<PropertyListing>> GetPagedProperties(PropertyFilters filters, bool published = true)
        {
            PagedList<PropertyListing> properties = new PagedList<PropertyListing>();
            var propertiesQuery = GetProperties(filters, published);
            properties.Items = (await propertiesQuery.ToListAsync()).Skip((filters.page - 1) * filters.pageSize).Take(filters.pageSize);
            properties.Count = propertiesQuery.Count();
            properties.Pages = properties.Count / filters.pageSize;
            if (properties.Pages < 1)
                properties.Pages = 1;
            if ((properties.Pages * filters.pageSize) < properties.Count)
            {
                properties.Pages++;
            }
            properties.CurrentPage = filters.page;
            properties.PageSize = filters.pageSize;
            return properties;
        }
        public Task<List<MapMarker>> GetLocations(PropertyFilters filters)
        {
            PagedList<PropertyListing> properties = new PagedList<PropertyListing>();
            var propertiesQuery = GetProperties(filters, true);
            return propertiesQuery.AsNoTracking().Select(p => new MapMarker(p, p.Title, p.Id.ToString(), p.Url)).ToListAsync();
        }
        public OperationResult<PropertyListing> UpdateProperty(PropertyListing property)
        {
            try
            {
                _db.Update(property);
                _db.SaveChanges();
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
                _db.Media.Add(new SiteMedia(media));
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
                _db.Media.Add(new SiteMedia(media));
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
            List<PropertyListing> properties = null;
            if (_cache.TryGetValue(cacheKey, out properties))
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
            List<PropertyListing> properties = null;
            if (_cache.TryGetValue(cacheKey, out properties))
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
    }
}
