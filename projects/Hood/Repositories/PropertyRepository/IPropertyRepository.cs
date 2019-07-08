using Hood.Enums;
using Hood.Models;
using Hood.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IPropertyRepository
    {
        #region Property CRUD
        Task<PropertyListModel> GetPropertiesAsync(PropertyListModel model);
        Task<List<MapMarker>> GetLocationsAsync(PropertyListModel filters);
        Task<PropertyListModel> GetFeaturedAsync();
        Task<PropertyListModel> GetRecentAsync();
        Task<PropertyListing> GetPropertyByIdAsync(int id, bool nocache = false);
        Task<PropertyListing> AddAsync(PropertyListing property);
        Task UpdateAsync(PropertyListing property);
        Task SetStatusAsync(int id, ContentStatus status);
        Task DeleteAsync(int id);
        Task DeleteAllAsync();
        #endregion

        #region Direct Field Editing
        Task ClearFieldAsync(int id, string field);
        #endregion

        #region Media / Floor Plans
        Task AddMediaAsync(PropertyListing property, PropertyMedia media);
        Task AddFloorplanAsync(PropertyListing property, PropertyFloorplan media);
        Task<PropertyListing> RemoveMediaAsync(int id, int mediaId);
        Task<PropertyListing> RemoveFloorplanAsync(int id, int mediaId);
        #endregion

        #region Statistics
        Task<object> GetStatisticsAsync();
        #endregion

    }
}