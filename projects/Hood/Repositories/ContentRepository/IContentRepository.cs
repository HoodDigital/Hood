using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hood.Enums;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hood.Services
{
    public interface IContentRepository
    {
        #region Content CRUD
        Task<ContentModel> GetContentAsync(ContentModel model);
        Task<Content> GetContentByIdAsync(int id, bool clearCache = false, bool track = true);
        Task<Content> AddAsync(Content content);
        Task<Content> UpdateAsync(Content content);
        Task DeleteAsync(int id);
        Task SetStatusAsync(int id, ContentStatus status);
        Task DeleteAllAsync(string type);
        Task<MediaDirectory> GetDirectoryAsync();
        #endregion

        #region Duplicate
        Task<Content> DuplicateContentAsync(int id);
        #endregion

        #region Images
        Task AddImageAsync(Content content, ContentMedia media);
        #endregion

        #region Content Views
        Task<ContentModel> GetRecentAsync(string type, string category = null);
        Task<ContentModel> GetFeaturedAsync(string type, string category = null);
        Task<ContentNeighbours> GetNeighbourContentAsync(int id, string type, string category = null);
        #endregion

        #region Categories 
        Task<ContentCategory> GetCategoryByIdAsync(int categoryId);
        Task<IEnumerable<ContentCategory>> GetCategoriesAsync(int contentId);
        Task<ContentCategory> AddCategoryAsync(string value, string type);
        Task<ContentCategory> AddCategoryAsync(ContentCategory category);
        Task DeleteCategoryAsync(int categoryId);
        Task UpdateCategoryAsync(ContentCategory category);
        Task AddCategoryToContentAsync(int contentId, int categoryId);
        Task RemoveCategoryFromContentAsync(int contentId, int categoryId);
        #endregion

        #region Sitemap
        // Sitemap
        Task<List<Content>> GetPages(string category = null);
        Task<string> GetSitemapDocumentAsync(IUrlHelper urlHelper);

        #endregion

        #region Metadata
        void UpdateTemplateMetas(Content content, List<string> newMetas);
        Task RefreshMetasAsync(Content content);
        Task RefreshAllMetasAsync();
        List<string> GetMetasForTemplate(string templateName, string folder);
        #endregion

        #region Helpers
        Task<bool> SlugExists(string slug, int? id = null);
        #endregion

        #region Statistics
        Task<object> GetStatisticsAsync();
        #endregion

        #region Obsoletes 
        [Obsolete("Use List<Country> Country.AllCountries from now on.", true)]
        List<Country> AllCountries();
        [Obsolete("Use static Country Country.GetCountry(string name) from now on.", true)]
        Country GetCountry(string name);
        #endregion

        #region LinqToTwitter
        Task<List<LinqToTwitter.Status>> GetTweets(string name, int count = 6);
        #endregion
    }
}