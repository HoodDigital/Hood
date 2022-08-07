using Hood.Enums;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IContentRepository
    {
        Task<Content> AddAsync(Content content);
        Task<ContentCategory> AddCategoryAsync(string value, string type);
        Task<ContentCategory> AddCategoryAsync(ContentCategory category);
        Task AddCategoryToContentAsync(int contentId, int categoryId);
        Task AddImageAsync(Content content, ContentMedia media);
        Task DeleteAllAsync(string type);
        Task DeleteAsync(int id);
        Task DeleteCategoryAsync(int categoryId);
        Task<Content> DuplicateContentAsync(int id);
        Task<IEnumerable<ContentCategory>> GetCategoriesAsync(int contentId);
        Task<ContentCategory> GetCategoryByIdAsync(int categoryId);
        Task<ContentModel> GetContentAsync(ContentModel model);
        Task<Content> GetContentByIdAsync(int id, bool clearCache = false, bool track = true);
        Task<MediaDirectory> GetDirectoryAsync();
        Task<ContentModel> GetFeaturedAsync(string type, string category = null, int pageSize = 5);
        List<string> GetMetasForTemplate(string templateName, string folder);
        Task<ContentNeighbours> GetNeighbourContentAsync(int id, string type, string category = null);
        Task<List<Content>> GetPages(string category = null);
        Task<ContentModel> GetRecentAsync(string type, string category = null, int pageSize = 5);
        Task<string> GetSitemapDocumentAsync(IUrlHelper urlHelper);
        Task<ContentStatitsics> GetStatisticsAsync();
        Task RefreshMetasAsync(Content content);
        Task RemoveCategoryFromContentAsync(int contentId, int categoryId);
        Task SetStatusAsync(int id, ContentStatus status);
        Task<bool> SlugExists(string slug, int? id = null);
        Task<Content> UpdateAsync(Content content);
        Task UpdateCategoryAsync(ContentCategory category);
        void UpdateTemplateMetas(Content content, List<string> newMetas);
    }
}
