﻿using Hood.Caching;
using Hood.Contexts;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly PropertyContext _db;
        private readonly HoodDbContext _hoodDb;
        private readonly IHoodCache _cache;
        private readonly IMediaManager _media;

        public PropertyRepository(
            PropertyContext db,
            HoodDbContext hoodDb,
            IHoodCache cache,
            IMediaManager media)
        {
            _db = db;
            _hoodDb = hoodDb;
            _cache = cache;
            _media = media;
        }

        #region Property CRUD
        public async Task<PropertyListModel> GetPropertiesAsync(PropertyListModel model)
        {
            IQueryable<PropertyListingView> properties = _db.PropertyViews
                .Include(p => p.Metadata);

            if (model.LoadImages)
            {
                properties = properties
                    .Include(p => p.Media)
                    .Include(p => p.FloorPlans);
            }

            if (model.PublishStatus.HasValue)
            {
                properties = properties.Where(p => p.Status == model.PublishStatus);
            }

            if (model.Type != null)
            {
                model.Type.RemoveAll(t => !t.IsSet());
                if (model.Type.Count > 0)
                {
                    properties = properties.Where(n => model.Type.Any(t => n.ListingType == t));
                }
            }

            if (model.Status != null)
            {
                model.Status.RemoveAll(t => !t.IsSet());
                if (model.Status.Count > 0)
                {
                    properties = properties.Where(n => model.Status.Any(t => n.LeaseStatus == t));
                }
            }

            if (model.Agent.IsSet())
            {
                properties = properties.Where(n => n.AgentEmail == model.Agent);
            }

            if (model.Location.IsSet())
            {
                properties = properties.Where(n => n.Address2 == model.Location);
            }

            if (model.PlanningType.IsSet())
            {
                properties = properties.Where(n => n.Planning == model.PlanningType);
            }

            if (model.MinBedrooms.HasValue)
            {
                properties = properties.Where(n => n.Bedrooms >= model.MinBedrooms.Value);
            }

            if (model.MaxBedrooms.HasValue)
            {
                properties = properties.Where(n => n.Bedrooms <= model.MaxBedrooms.Value);
            }

            if (model.Bedrooms.HasValue)
            {
                properties = properties.Where(n => n.Bedrooms == model.Bedrooms.Value);
            }

            if (model.MinRent.HasValue)
            {
                properties = properties.Where(n => n.Rent >= model.MinRent.Value);
            }

            if (model.MaxRent.HasValue)
            {
                properties = properties.Where(n => n.Rent <= model.MaxRent.Value);
            }

            if (model.MinPremium.HasValue)
            {
                properties = properties.Where(n => n.Premium >= model.MinPremium.Value);
            }

            if (model.MaxPremium.HasValue)
            {
                properties = properties.Where(n => n.Premium <= model.MaxPremium.Value);
            }

            if (model.MinPrice.HasValue)
            {
                properties = properties.Where(n => n.AskingPrice >= model.MinPrice.Value);
            }

            if (model.MaxPrice.HasValue)
            {
                properties = properties.Where(n => n.AskingPrice <= model.MaxPrice.Value);
            }

            if (model.Search.IsSet())
            {
                properties = properties.Where(n =>
                    n.Title.Contains(model.Search) ||
                    n.Address1.Contains(model.Search) ||
                    n.Address2.Contains(model.Search) ||
                    n.City.Contains(model.Search) ||
                    n.County.Contains(model.Search) ||
                    n.Postcode.Contains(model.Search) ||
                    n.ShortDescription.Contains(model.Search) ||
                    n.Lease.Contains(model.Search) ||
                    n.Location.Contains(model.Search) ||
                    n.Planning.Contains(model.Search) ||
                    n.Reference.Contains(model.Search)
                );
            }

            if (model.Order.IsSet())
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

            model.AvailableTypes = await _db.Properties.Select(p => p.ListingType).Distinct().ToListAsync();
            model.AvailableStatuses = await _db.Properties.Select(p => p.LeaseStatus).Distinct().ToListAsync();
            model.AvailablePlanningTypes = await _db.Properties.Select(p => p.Planning).Distinct().ToListAsync();
            model.PlanningTypes = Engine.Settings.Property.GetPlanningTypes();

            await model.ReloadAsync(properties);
            if (model.List != null)
            {
                model.Locations = model.List.Select(p => new MapMarker(p, p.Title, p.QuickInfo, p.Id.ToString(), p.Url, p.FeaturedImage.Url)).ToList();
                model.CentrePoint = GeoCalculations.GetCentralGeoCoordinate(model.Locations.Select(p => new GeoCoordinate(p.Latitude, p.Longitude)));
            }
            return model;
        }
        public async Task<List<MapMarker>> GetLocationsAsync(PropertyListModel filters)
        {
            PropertyListModel propertiesQuery = await GetPropertiesAsync(filters);
            return propertiesQuery.List.Select(p =>
                new MapMarker(p, p.Title, p.QuickInfo, p.Id.ToString(), p.Url, p.FeaturedImage.Url)
            ).ToList();
        }
        public async Task<PropertyListModel> GetFeaturedAsync()
        {
            string cacheKey = typeof(PropertyListModel) + ".Featured";
            if (_cache.TryGetValue(cacheKey, out PropertyListModel properties))
            {
                return properties;
            }
            else
            {
                properties = await GetPropertiesAsync(new PropertyListModel() { Featured = true, PageSize = int.MaxValue });
                _cache.Add(cacheKey, properties, TimeSpan.FromMinutes(60));
                return properties;
            }
        }
        public async Task<PropertyListModel> GetRecentAsync()
        {
            string cacheKey = typeof(PropertyListModel) + ".Recent";
            if (_cache.TryGetValue(cacheKey, out PropertyListModel properties))
            {
                return properties;
            }
            else
            {
                properties = await GetPropertiesAsync(new PropertyListModel() { PageSize = int.MaxValue, Order = "DateDesc" });
                _cache.Add(cacheKey, properties, TimeSpan.FromMinutes(60));
                return properties;
            }
        }
        public async Task<PropertyListing> GetPropertyByIdAsync(int id, bool nocache = false)
        {
            PropertyListing property = await _db.Properties
                    .Include(p => p.Media)
                    .Include(p => p.FloorPlans)
                    .Include(p => p.Metadata)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

            return property;
        }
        public async Task<PropertyListingView> GetPropertyViewByIdAsync(int id, bool nocache = false)
        {
            PropertyListingView property = await _db.PropertyViews
                    .Include(p => p.Media)
                    .Include(p => p.FloorPlans)
                    .Include(p => p.Metadata)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

            return property;
        }
        public async Task<PropertyListing> ReloadReferences(PropertyListing property)
        {
            _db.Entry(property).State = EntityState.Added;
            await _db.Entry(property).Collection(s => s.Media).LoadAsync();
            await _db.Entry(property).Collection(s => s.FloorPlans).LoadAsync();
            await _db.Entry(property).Collection(s => s.Metadata).LoadAsync();
            return property;
        }
        public async Task<PropertyListing> AddAsync(PropertyListing property)
        {
            _db.Properties.Add(property);
            await _db.SaveChangesAsync();
            return property;
        }
        public async Task UpdateAsync(PropertyListing property)
        {
            _db.Entry(property).State = EntityState.Modified;
            await _db.SaveChangesAsync();
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
            Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<PropertyListing, List<PropertyFloorplan>> all = _db.Properties
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
            MediaDirectory contentDirectory = await _hoodDb.MediaDirectories.SingleOrDefaultAsync(md => md.Slug == MediaManager.PropertyDirectorySlug && md.Type == DirectoryType.System);
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
            {
                property.Media = new List<PropertyMedia>();
            }

            property.Media.Add(media);
            _db.Properties.Update(property);
            await _db.SaveChangesAsync();
            ClearPropertyCache(property.Id);
        }
        public async Task AddFloorplanAsync(PropertyListing property, PropertyFloorplan media)
        {
            if (property.FloorPlans == null)
            {
                property.FloorPlans = new List<PropertyFloorplan>();
            }

            property.FloorPlans.Add(media);
            _db.Properties.Update(property);
            await _db.SaveChangesAsync();
            ClearPropertyCache(property.Id);
        }
        public async Task<PropertyListing> RemoveMediaAsync(int id, int mediaId)
        {
            PropertyListing property = await _db.Properties.Include(p => p.Media).SingleOrDefaultAsync(p => p.Id == id);
            PropertyMedia media = property.Media.SingleOrDefault(m => m.Id == mediaId);
            if (media != null)
            {
                property.Media.Remove(media);
            }

            await _db.SaveChangesAsync();
            return property;
        }
        public async Task<PropertyListing> RemoveFloorplanAsync(int id, int mediaId)
        {
            PropertyListing property = await _db.Properties.Include(p => p.FloorPlans).SingleOrDefaultAsync(p => p.Id == id);
            PropertyFloorplan media = property.FloorPlans.SingleOrDefault(m => m.Id == mediaId);
            if (media != null)
            {
                property.FloorPlans.Remove(media);
            }

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
            {
                _cache.Remove("Property:Listing:" + id.ToString());
            }
        }
        #endregion

        #region Statistics
        public async Task<PropertyStatistics> GetStatisticsAsync()
        {
            int totalProperties = await _db.Properties.CountAsync();
            int totalPublished = await _db.Properties.Where(c => c.Status == ContentStatus.Published && c.PublishDate < DateTime.UtcNow).CountAsync();
            var data = await _db.Properties.Where(p => p.CreatedOn >= DateTime.Now.AddYears(-1)).Select(c => new { date = c.CreatedOn.Date, month = c.CreatedOn.Month, pubdate = c.PublishDate.Date, pubmonth = c.PublishDate.Month }).ToListAsync();

            var createdByDate = data.GroupBy(p => p.date).Select(g => new { name = g.Key, count = g.Count() });
            var createdByMonth = data.GroupBy(p => p.month).Select(g => new { name = g.Key, count = g.Count() });
            var publishedByDate = data.GroupBy(p => p.pubdate).Select(g => new { name = g.Key, count = g.Count() });
            var publishedByMonth = data.GroupBy(p => p.pubmonth).Select(g => new { name = g.Key, count = g.Count() });

            List<KeyValuePair<string, int>> days = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> publishDays = new List<KeyValuePair<string, int>>();
            foreach (DateTime day in DateTimeExtensions.EachDay(DateTime.UtcNow.AddDays(-89), DateTime.UtcNow))
            {
                var dayvalue = createdByDate.SingleOrDefault(c => c.name == day.Date);
                int count = dayvalue != null ? dayvalue.count : 0;
                days.Add(new KeyValuePair<string, int>(day.ToString("dd MMM"), count));

                dayvalue = publishedByDate.SingleOrDefault(c => c.name == day.Date);
                count = dayvalue != null ? dayvalue.count : 0;
                publishDays.Add(new KeyValuePair<string, int>(day.ToString("dd MMM"), count));
            }

            List<KeyValuePair<string, int>> months = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> publishMonths = new List<KeyValuePair<string, int>>();
            for (DateTime dt = DateTime.UtcNow.AddMonths(-11); dt <= DateTime.UtcNow; dt = dt.AddMonths(1))
            {
                var monthvalue = createdByMonth.SingleOrDefault(c => c.name == dt.Month);
                int count = monthvalue != null ? monthvalue.count : 0;
                months.Add(new KeyValuePair<string, int>(dt.ToString("MMMM, yyyy"), count));

                monthvalue = publishedByMonth.SingleOrDefault(c => c.name == dt.Month);
                count = monthvalue != null ? monthvalue.count : 0;
                publishMonths.Add(new KeyValuePair<string, int>(dt.ToString("MMMM, yyyy"), count));
            }

            return new PropertyStatistics(totalProperties, totalPublished, days, months, publishDays, publishMonths);
        }
        #endregion
    }

    public class PropertyStatistics
    {
        public PropertyStatistics()
        {
        }

        public PropertyStatistics(int totalProperties, int totalPublished, List<KeyValuePair<string, int>> days, List<KeyValuePair<string, int>> months, List<KeyValuePair<string, int>> publishDays, List<KeyValuePair<string, int>> publishMonths)
        {
            TotalProperties = totalProperties;
            TotalPublished = totalPublished;
            Days = days;
            Months = months;
            PublishDays = publishDays;
            PublishMonths = publishMonths;
        }

        public int TotalProperties { get; set; }
        public int TotalPublished { get; set; }
        public List<KeyValuePair<string, int>> Days { get; set; }
        public List<KeyValuePair<string, int>> Months { get; set; }
        public List<KeyValuePair<string, int>> PublishDays { get; set; }
        public List<KeyValuePair<string, int>> PublishMonths { get; set; }
    }
}
