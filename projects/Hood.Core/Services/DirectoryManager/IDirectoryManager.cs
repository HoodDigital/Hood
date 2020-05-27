using System.Collections.Generic;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Html;

namespace Hood.Services
{
    public interface IDirectoryManager
    {

        int Count();
        string GetPath(int? id);
        void ResetCache();
        MediaDirectory GetDirectoryById(int id);
        IEnumerable<MediaDirectory> TopLevel();
        IEnumerable<MediaDirectory> MediaDirectories();
        IEnumerable<MediaDirectory> UserDirectories(string userId);
        IEnumerable<MediaDirectory> GetHierarchy(int id, int? stopAtId = null);
        IEnumerable<MediaDirectory> GetAllCategoriesIncludingChildren(IEnumerable<MediaDirectory> startLevel);
        MediaDirectory GetTopLevelDirectory(int id);
        IHtmlContent GetBreadcrumb(MediaListModel model, string targetListDOMObject = "#media-list");
        IHtmlContent SelectOptions(IEnumerable<MediaDirectory> startLevel, int? selectedValue, int startingLevel = 0);
        IHtmlContent AdminDirectoryTree(IEnumerable<MediaDirectory> startLevel, int? selectedValue, int startingLevel = 0);
        IHtmlContent DirectoryTree(IEnumerable<MediaDirectory> startLevel, int? selectedValue, int startingLevel = 0);
    }
}