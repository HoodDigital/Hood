using System;
using System.Collections.Generic;
using System.Linq;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Hood.Services
{
    public class DirectoryManager : IDirectoryManager
    {
        private readonly IConfiguration _config;
        private readonly object _cacheLock = new object();

        // Ids confirmed absent after a reload — suppresses repeated full reloads for stale references.
        private volatile HashSet<int> _confirmedAbsent = new HashSet<int>();

        // Single holder published atomically; readers capture all three Lazy fields in one volatile read.
        private volatile CacheSnapshot _cache;

        private sealed class CacheSnapshot
        {
            internal readonly Lazy<Dictionary<int, MediaDirectory>> ById;
            internal readonly Lazy<MediaDirectory[]> TopLevel;
            internal readonly Lazy<MediaDirectory> SiteDirectory;

            internal CacheSnapshot(
                Lazy<Dictionary<int, MediaDirectory>> byId,
                Lazy<MediaDirectory[]> topLevel,
                Lazy<MediaDirectory> siteDirectory
            )
            {
                ById = byId;
                TopLevel = topLevel;
                SiteDirectory = siteDirectory;
            }
        }

        public DirectoryManager(IConfiguration config)
        {
            _config = config;
            ResetCache();
        }

        public int Count()
        {
            return _cache.ById.Value.Count;
        }

        public void ResetCache()
        {
            DbContextOptionsBuilder<HoodDbContext> options =
                new DbContextOptionsBuilder<HoodDbContext>();
            options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
            using HoodDbContext db = new HoodDbContext(options.Options);

            var allDirectories = (
                from d in db.MediaDirectories
                select new MediaDirectory
                {
                    Id = d.Id,
                    DisplayName = d.DisplayName,
                    Slug = d.Slug,
                    Type = d.Type,
                    OwnerId = d.OwnerId,
                    ParentId = d.ParentId,
                    Parent = d.Parent,
                    Children = d.Children,
                }
            ).ToDictionary(c => c.Id);

            var newById = new Lazy<Dictionary<int, MediaDirectory>>(() => allDirectories);
            var newTopLevel = new Lazy<MediaDirectory[]>(() =>
                allDirectories.Values.Where(c => c.ParentId == null).ToArray()
            );
            var newSiteDirectory = new Lazy<MediaDirectory>(() =>
                allDirectories.Values.SingleOrDefault(c =>
                    c.Slug == MediaManager.SiteDirectorySlug && c.Type == DirectoryType.System
                )
            );

            lock (_cacheLock)
            {
                _cache = new CacheSnapshot(newById, newTopLevel, newSiteDirectory);
                _confirmedAbsent = new HashSet<int>();
            }
        }

        public MediaDirectory GetDirectoryById(int id)
        {
            if (!_cache.ById.Value.ContainsKey(id))
            {
                return null;
            }

            return _cache.ById.Value[id];
        }

        public IEnumerable<MediaDirectory> MediaDirectories()
        {
            CacheSnapshot snap = _cache;
            return snap
                .ById.Value.Values.Where(c => c.ParentId == snap.SiteDirectory.Value.Id)
                .ToArray();
        }

        public IEnumerable<MediaDirectory> UserDirectories(string userId)
        {
            CacheSnapshot snap = _cache;
            return snap
                .ById.Value.Values.Where(c =>
                    c.ParentId != null && c.Parent.Type == DirectoryType.User && c.OwnerId == userId
                )
                .ToArray();
        }

        public IEnumerable<MediaDirectory> TopLevel()
        {
            return _cache.TopLevel.Value;
        }

        public IEnumerable<MediaDirectory> GetHierarchy(int id, int? stopAtId = null)
        {
            // Reload once when the id is absent — picks up newly-created directories.
            // If the id is still missing after a reload it is recorded so subsequent
            // calls skip the DB round-trip entirely.
            if (!_cache.ById.Value.ContainsKey(id) && !_confirmedAbsent.Contains(id))
            {
                ResetCache();
                if (!_cache.ById.Value.ContainsKey(id))
                {
                    lock (_cacheLock)
                    {
                        if (!_confirmedAbsent.Contains(id))
                        {
                            var absent = new HashSet<int>(_confirmedAbsent) { id };
                            _confirmedAbsent = absent;
                        }
                    }
                }
            }

            List<MediaDirectory> result = new List<MediaDirectory>();
            MediaDirectory directory = GetDirectoryById(id);
            while (directory != null)
            {
                result.Insert(0, directory);

                if (stopAtId.HasValue && directory.Id == stopAtId.Value)
                {
                    return result;
                }

                directory = directory.Parent;
            }

            return result;
        }

        public IEnumerable<MediaDirectory> GetAllCategoriesIncludingChildren(
            IEnumerable<MediaDirectory> startLevel
        )
        {
            var directories = startLevel.ToList();
            return directories.Union(
                directories
                    .Where(c => c.Children != null)
                    .SelectMany(c => GetAllCategoriesIncludingChildren(c.Children))
            );
        }

        public MediaDirectory GetTopLevelDirectory(int id)
        {
            MediaDirectory result = GetDirectoryById(id);
            while (result.Parent != null)
            {
                result = result.Parent;
            }
            return result;
        }

        public string GetPath(int? id)
        {
            if (id.HasValue)
            {
                return string.Join("/", GetHierarchy(id.Value).Select(x => x.Slug).ToArray());
            }

            return "/";
        }

        public IHtmlContent GetBreadcrumb(
            MediaListModel model,
            string targetListDOMObject = "#media-list"
        )
        {
            List<string> links = new List<string>();

            LinkGenerator linkGenerator = Core.Engine.Services.Resolve<LinkGenerator>();
            string baseUrl = linkGenerator.GetPathByAction("List", "Media", new { area = "Admin" });

            MediaListModel linkModel = new MediaListModel();
            model.CopyProperties(linkModel);
            string link;

            if (!model.RootId.HasValue)
            {
                linkModel.DirectoryId = null;
                link = linkModel.GetPageUrl(linkModel.PageIndex);
                links.Add(
                    $"<a class=\"hood-inline-list-target\" data-target=\"{targetListDOMObject}\" href=\"{baseUrl}{link}\">Everything</a>"
                );
            }

            if (!model.DirectoryId.HasValue)
            {
                return FormatBreadcrumbLinks(links);
            }

            foreach (
                MediaDirectory directory in GetHierarchy(model.DirectoryId.Value, model.RootId)
            )
            {
                linkModel.DirectoryId = directory.Id;
                link = linkModel.GetPageUrl(linkModel.PageIndex);
                var linkTitle = directory.DisplayName.IsSet() ? directory.DisplayName : "Untitled";
                links.Add(
                    $"<a class=\"hood-inline-list-target\" data-target=\"{targetListDOMObject}\" href=\"{baseUrl}{link}\">{linkTitle}</a>"
                );
            }
            return FormatBreadcrumbLinks(links);
        }

        private static IHtmlContent FormatBreadcrumbLinks(List<string> links)
        {
            string htmlOutput = string.Join(
                " <i class=\"fa fa-caret-right mx-2\"></i> ",
                links.ToArray()
            );
            HtmlString builder = new HtmlString(htmlOutput);
            return builder;
        }

        // Html
        public IHtmlContent SelectOptions(
            IEnumerable<MediaDirectory> startLevel,
            int? selectedValue,
            int startingLevel = 0
        )
        {
            string htmlOutput = string.Empty;
            var directories = startLevel?.ToList();
            if (directories != null && directories.Count > 0)
            {
                foreach (int key in directories.Select(c => c.Id))
                {
                    // Have to reload from the cache to use the count.
                    MediaDirectory directory = GetDirectoryById(key);

                    htmlOutput +=
                        "<option value=\""
                        + directory.Id
                        + "\""
                        + (selectedValue == directory.Id ? " selected" : "")
                        + ">";
                    for (int i = 0; i < startingLevel; i++)
                    {
                        htmlOutput += "- ";
                    }
                    htmlOutput += string.Format("{0}", directory.DisplayName);
                    htmlOutput += "</option>";
                    htmlOutput += SelectOptions(
                        directory.Children,
                        selectedValue,
                        startingLevel + 1
                    );
                }
            }

            HtmlString builder = new HtmlString(htmlOutput);
            return builder;
        }

        public IHtmlContent AdminDirectoryTree(
            IEnumerable<MediaDirectory> startLevel,
            int? selectedValue,
            int startingLevel = 0
        )
        {
            string htmlOutput = string.Empty;

            var directories = startLevel?.ToList();
            if (directories != null && directories.Count > 0)
            {
                foreach (int key in directories.Select(c => c.Id))
                {
                    // Have to reload from the cache to use the count.
                    MediaDirectory directory = GetDirectoryById(key);

                    string carets = "";
                    for (int i = 0; i < startingLevel; i++)
                    {
                        carets += "<i class='fa fa-caret-right mr-1'></i>";
                    }

                    string check = (selectedValue == directory.Id ? " checked" : "");

                    string template =
                        $@"

    <div class='list-group-item list-group-item-action p-0'>
        <div class='custom-control custom-checkbox d-flex'>
            <input class='custom-control-input refresh-on-change'
                   id='Directory-{directory.Slug}' name='dir'
                   type='radio'
                   value='{directory.Id}' {check} />
            <label class='custom-control-label col m-2 mt-1 mb-1' for='Directory-{directory.Slug}'>
                {carets}{directory.DisplayName}
            </label>
            <div class='col-auto p-2'>
                <a class='btn-link text-danger content-directories-delete' href='/admin/media/directory/delete?id={directory.Id}'>
                    <i class='fa fa-trash'></i>
                    <span>
                        Delete
                    </span>
                </a>
            </div>
        </div>
    </div>

";
                    htmlOutput += "";
                    htmlOutput += template;
                    htmlOutput += AdminDirectoryTree(directory.Children, startingLevel + 1);
                }
            }

            HtmlString builder = new HtmlString(htmlOutput);
            return builder;
        }

        public IHtmlContent DirectoryTree(
            IEnumerable<MediaDirectory> startLevel,
            int? selectedValue,
            int startingLevel = 0
        )
        {
            string htmlOutput = string.Empty;

            var directories = startLevel?.ToList();
            if (directories != null && directories.Count > 0)
            {
                foreach (int key in directories.Select(c => c.Id))
                {
                    // Have to reload from the cache to use the count.
                    MediaDirectory directory = GetDirectoryById(key);

                    string carets = "";
                    for (int i = 0; i < startingLevel; i++)
                    {
                        carets += "<span class='d-inline-block ml-1'>&nbsp;</span>";
                    }

                    string expanded = "false";
                    string template = "<div class='list-group-item list-group-item-action p-0'>";
                    if (startingLevel == 0)
                    {
                        expanded = "true";
                    }
                    else
                    {
                        template =
                            $@"<div class='list-group-item list-group-item-action p-0 collapse'
                                           id='sub-directory-{directory.ParentId}'
                                           aria-labelledby='sub-directory-heading-{directory.ParentId}'>";
                    }

                    string expand = $"<small><i class='fa fa-square p-1'></i></small>";
                    if (directory.Children != null && directory.Children.Count > 0)
                    {
                        expand =
                            $@"
                            <a class='btn-link' data-toggle='collapse' aria-labelledby='sub-directory-heading-{directory.Id}' data-target='#sub-directory-{directory.Id}' href='#sub-directory-{directory.Id}' aria-expanded='{expanded}' aria-controls='sub-directory-{directory.Id}'>
                                <small><i class='fa fa-plus-square p-1 text-success'></i></small>
                            </a>";
                    }

                    string check = (selectedValue == directory.Id ? "checked" : "");

                    template +=
                        $@"
                            <div class='d-flex align-items-center'>
                                <div class='col-auto p-2'>
                                    {carets}{expand}
                                </div>
                                <div class='col p-0'>
                                    <div class='custom-control custom-checkbox'>
                                        <input class='custom-control-input refresh-on-change'
                                                id='Directory-{directory.Id}' name='dir'
                                                type='radio'
                                                value='{directory.Id}' {check} />
                                        <label class='custom-control-label' for='Directory-{directory.Id}'>
                                            {directory.DisplayName}
                                        </label>
                                    </div>
                                </div>
                                <div class='col-auto p-2'>
                                    <a class='btn-link text-danger media-directories-delete' href='/admin/media/directory/delete?id={directory.Id}'>
                                        <i class='fa fa-trash'></i>
                                        <span>
                                            Delete
                                        </span>
                                    </a>
                                </div>
                            </div>
                        </div>";

                    htmlOutput += "";
                    htmlOutput += template;
                    htmlOutput += DirectoryTree(
                        directory.Children,
                        selectedValue,
                        startingLevel + 1
                    );
                }
            }

            HtmlString builder = new HtmlString(htmlOutput);
            return builder;
        }
    }
}
