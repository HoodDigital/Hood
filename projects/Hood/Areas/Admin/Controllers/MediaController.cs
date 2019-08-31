using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin,Editor")]
    public class MediaController : BaseController
    {
        public MediaController()
            : base()
        {
        }

        [Route("admin/media/list/")]
        public async Task<IActionResult> List(MediaListModel model, string viewName = "_List_Media")
        {
            IQueryable<MediaObject> media = _db.Media.AsQueryable();

            if (model.GenericFileType.HasValue)
            {
                media = media.Where(m => m.GenericFileType == model.GenericFileType);
            }
            else
            {
                media = media.Where(m => m.GenericFileType != GenericFileType.Directory);
            }

            if (model.DirectoryId.HasValue)
            {
                var directory = _directoryManager.GetDirectoryById(model.DirectoryId.Value);
                if (directory != null)
                {
                    var tree = _directoryManager.GetAllCategoriesIncludingChildren(new List<MediaDirectory>() { directory });
                    media = media.Where(n => tree.Any(t => t.Id == n.DirectoryId));
                }
            }
            else
            {
                var directory = await _db.MediaDirectories.SingleOrDefaultAsync(o => o.Slug == MediaManager.SiteDirectorySlug && o.Type == DirectoryType.System);
                directory = _directoryManager.GetDirectoryById(directory.Id);
                var tree = _directoryManager.GetAllCategoriesIncludingChildren(new List<MediaDirectory>() { directory });
                media = media.Where(n => tree.Any(t => t.Id == n.DirectoryId));
                model.DirectoryId = directory.Id;
            }

            if (model.UserId.IsSet())
            {
                media = media.Where(n => n.Directory.OwnerId == model.UserId);
            }

            if (model.Search.IsSet())
            {
                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                media = media.Where(n => searchTerms.Any(s => n.Filename.ToLower().Contains(s.ToLower())));
            }

            switch (model.Order)
            {
                case "Size":
                    media = media.OrderBy(n => n.FileSize);
                    break;
                case "Filename":
                    media = media.OrderBy(n => n.Filename);
                    break;
                case "Date":
                    media = media.OrderBy(n => n.CreatedOn);
                    break;

                case "SizeDesc":
                    media = media.OrderByDescending(n => n.FileSize);
                    break;
                case "FilenameDesc":
                    media = media.OrderByDescending(n => n.Filename);
                    break;
                case "DateDesc":
                    media = media.OrderByDescending(n => n.CreatedOn);
                    break;

                default:
                    media = media.OrderByDescending(n => n.CreatedOn);
                    break;
            }

            await model.ReloadAsync(media);
            model.TopLevelDirectories = GetDirectoriesForCurrentUser();
            return View(viewName, model);
        }

        [Route("admin/media/")]
        public async Task<IActionResult> Index(MediaListModel model)
        {
            return await List(model, "Index");
        }

        [Route("admin/media/action/")]
        public async Task<IActionResult> Action(MediaListModel model)
        {
            return await List(model, "Action");
        }

        [Route("admin/media/blade/")]
        public async Task<IActionResult> Blade(int id)
        {
            MediaObject media = await _db.Media.Include(m => m.Directory).SingleOrDefaultAsync(u => u.Id == id);
            return View("_Blade_Media", media);
        }

        [HttpPost]
        [Route("admin/media/delete/")]
        public async Task<Response> Delete(int id)
        {
            try
            {
                MediaObject media = _db.Media.SingleOrDefault(m => m.Id == id);
                try { await _media.DeleteStoredMedia(media); } catch (Exception) { }
                _db.Media.Remove(media);
                await _db.SaveChangesAsync();
                _directoryManager.ResetCache();
                return new Response(true, $"The media has been deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<MediaController>($"Error deleting a media object.", ex);
            }
        }

        #region Directories
        [Route("admin/media/directories/list/")]
        public IActionResult Directories(MediaListModel model)
        {
            model.TopLevelDirectories = GetDirectoriesForCurrentUser();
            return View("_List_Directories", model);
        }

        private IEnumerable<MediaDirectory> GetDirectoriesForCurrentUser()
        {
            if (User.IsAdminOrBetter())
            {
                return _directoryManager.TopLevel();
            }
            else if (User.IsEditorOrBetter())
            {
                return _directoryManager.MediaDirectories();
            }
            else
            {
                return _directoryManager.UserDirectories(User.GetUserId());
            }
        }

        [Route("admin/media/directory/create/")]
        public IActionResult CreateDirectory()
        {
            MediaDirectory model = new MediaDirectory
            {
                TopLevelDirectories = GetDirectoriesForCurrentUser()
            };
            return View("_Blade_Directory", model);
        }

        [HttpPost]
        [Route("admin/media/directory/create/")]
        public async Task<Response> CreateDirectory(MediaDirectory model)
        {
            try
            {
                if (!model.ParentId.HasValue)
                {
                    throw new Exception("You must select a parent directory to create your directory in.");
                }

                MediaDirectory parentDirectory = await _db.MediaDirectories.SingleOrDefaultAsync(md => md.Id == model.ParentId.Value);
                if (parentDirectory == null)
                {
                    throw new Exception($"The parent directory with id {model.ParentId} could not be found.");
                }

                MediaDirectory root = _directoryManager.GetTopLevelDirectory(parentDirectory.Id);
                if (root.Type != DirectoryType.System || root.Slug != MediaManager.SiteDirectorySlug)
                {
                    throw new Exception("You cannot create directories outside the site media folders.");
                }

                if (!User.IsEditorOrBetter())
                {
                    if (root.Type != DirectoryType.System || root.Slug != MediaManager.UserDirectorySlug)
                    {
                        throw new Exception("You cannot create directories outside the user folders folders.");
                    }

                    if (parentDirectory.OwnerId != User.GetUserId())
                    {
                        throw new Exception("You cannot create directories outside your own folder.");
                    }
                }

                model.OwnerId = User.GetUserId();
                model.Type = DirectoryType.Site;
                model.Slug = model.DisplayName.ToSeoUrl();

                _db.MediaDirectories.Add(model);
                await _db.SaveChangesAsync();
                _directoryManager.ResetCache();
                return new Response(true, "The directory has been created and you can add files to it right away.");
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }

        }

        [HttpPost]
        [Route("admin/media/directory/delete/")]
        public async Task<Response> DeleteDirectory(int id)
        {
            try
            {
                MediaDirectory directory = await _db.MediaDirectories.Include(md => md.Children).SingleOrDefaultAsync(md => md.Id == id);

                if (directory == null)
                    throw new Exception("You have to select a directory to delete.");

                if (directory.Type == DirectoryType.System)
                    throw new Exception("You cannot delete a system directory.");

                if (directory.Type == DirectoryType.User)
                    throw new Exception("You cannot delete a user directory.");

                IEnumerable<MediaDirectory> directories = _directoryManager.GetAllCategoriesIncludingChildren(new List<MediaDirectory>() { directory });

                foreach (MediaDirectory dir in directories)
                {
                    IQueryable<MediaObject> directoryList = _db.Media.AsQueryable();
                    directoryList = directoryList.Where(m => m.DirectoryId == dir.Id);

                    foreach (MediaObject media in directoryList)
                    {
                        try { await _media.DeleteStoredMedia(media); } catch (Exception) { }
                        _db.Entry(media).State = EntityState.Deleted;
                    }
                }

                directory.Children.ForEach(c => _db.Entry(c).State = EntityState.Deleted);
                await _db.SaveChangesAsync();

                _db.Entry(directory).State = EntityState.Deleted;
                await _db.SaveChangesAsync();

                _directoryManager.ResetCache();
                return new Response(true, $"The directory has been deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<MediaController>($"Error deleting a directory.", ex);
            }
        }
        #endregion

        #region Attaching To Entities 
        /// <summary>
        /// Attach media file to entity. This is the action which handles the chosen attachment from the CHOOSE -> ATTACH modal.
        /// </summary>
        [HttpPost]
        [Route("admin/media/attach/")]
        public async Task<Response> Attach(MediaListModel model)
        {
            try
            {
                // load the media object.
                MediaObject media = _db.Media.SingleOrDefault(m => m.Id == model.MediaId);
                string cacheKey;
                switch (model.Entity)
                {
                    case "Content":

                        // create the new media item for content =>
                        int contentId = int.Parse(model.Id);
                        Content content = await _db.Content.Where(p => p.Id == contentId).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case "FeaturedImage":
                                content.FeaturedImage = new ContentMedia(media);
                                break;
                            case "ShareImage":
                                content.ShareImage = new ContentMedia(media);
                                break;
                        }

                        cacheKey = typeof(Content).ToString() + ".Single." + contentId;
                        _cache.Remove(cacheKey);

                        break;

                    case "Forum":

                        // create the new media item for content =>
                        int forumId = int.Parse(model.Id);
                        Forum forum = await _db.Forums.Where(p => p.Id == forumId).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case "FeaturedImage":
                                forum.FeaturedImage = new ContentMedia(media);
                                break;
                            case "ShareImage":
                                forum.ShareImage = new ContentMedia(media);
                                break;
                        }

                        break;

                    case "ApplicationUser":

                        // create the new media item for content =>
                        ApplicationUser user = await _db.Users.Where(p => p.Id == model.Id).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case "Avatar":
                                user.Avatar = media;
                                break;
                        }


                        cacheKey = typeof(ApplicationUser).ToString() + ".Single." + model.Id;
                        _cache.Remove(cacheKey);

                        break;

                    case "PropertyListing":

                        int propertyId = int.Parse(model.Id);
                        PropertyListing property = await _db.Properties.Where(p => p.Id == propertyId).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case "FeaturedImage":
                                property.FeaturedImage = media;
                                break;
                            case "InfoDownload":
                                property.InfoDownload = media;
                                break;
                        }

                        cacheKey = typeof(PropertyListing).ToString() + ".Single." + propertyId;
                        _cache.Remove(cacheKey);

                        break;

                    case nameof(Subscription):

                        // create the new media item for content =>
                        int subId = int.Parse(model.Id);
                        Subscription subscription = await _db.Subscriptions.Where(p => p.Id == subId).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case nameof(Subscription.FeaturedImage):
                                subscription.FeaturedImage = media;
                                break;
                        }

                        break;

                    case nameof(SubscriptionProduct):

                        // create the new media item for content =>
                        int subgId = int.Parse(model.Id);
                        SubscriptionProduct subscriptionGroup = await _db.SubscriptionProducts.Where(p => p.Id == subgId).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case nameof(SubscriptionProduct.FeaturedImage):
                                subscriptionGroup.FeaturedImage = media;
                                break;
                        }

                        break;

                    case "ContentMeta":

                        int idForMeta = int.Parse(model.Id);
                        Content contentForMeta = await _db.Content.Include(c => c.Metadata).Where(p => p.Id == idForMeta).FirstOrDefaultAsync();
                        MediaObject mi = await _db.Media.Where(m => m.Id == model.MediaId).FirstOrDefaultAsync();
                        contentForMeta.UpdateMeta(model.Field, mi);

                        cacheKey = typeof(Content).ToString() + ".Single." + idForMeta;
                        _cache.Remove(cacheKey);

                        break;

                    default:
                        throw new Exception("No field set to attach the media item to.");
                }

                await _db.SaveChangesAsync();
                return new Response(true, media, $"The media has been set for attached successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<MediaController>($"Error attaching a media file to an entity.", ex);
            }
        }

        /// <summary>
        /// Attach media file to entity. This is the action which handles the chosen attachment from the CHOOSE -> ATTACH modal.
        /// </summary>
        [HttpPost]
        [Route("admin/media/clear/")]
        public async Task<Response> Clear(MediaListModel model)
        {
            try
            {
                // load the media object.
                string cacheKey;
                switch (model.Entity)
                {
                    case nameof(Models.Content):

                        // create the new media item for content =>
                        int contentId = int.Parse(model.Id);
                        Content content = await _db.Content.Where(p => p.Id == contentId).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case nameof(Models.Content.FeaturedImage):
                                content.FeaturedImageJson = null;
                                break;
                            case nameof(Models.Content.ShareImage):
                                content.ShareImageJson = null;
                                break;
                        }

                        await _db.SaveChangesAsync();
                        cacheKey = typeof(Content).ToString() + ".Single." + contentId;
                        _cache.Remove(cacheKey);
                        return new Response(true, MediaObject.Blank, $"The media file has been removed successfully.");

                    case nameof(Forum):

                        // create the new media item for content =>
                        int forumId = int.Parse(model.Id);
                        Forum forum = await _db.Forums.Where(p => p.Id == forumId).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case nameof(Forum.FeaturedImage):
                                forum.FeaturedImage = null;
                                break;
                            case nameof(Forum.ShareImage):
                                forum.ShareImage = null;
                                break;
                        }

                        await _db.SaveChangesAsync();
                        return new Response(true, MediaObject.Blank, $"The media file has been removed successfully.");

                    case nameof(ApplicationUser):

                        // create the new media item for content =>
                        ApplicationUser user = await _db.Users.Where(p => p.Id == model.Id).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case nameof(ApplicationUser.Avatar):
                                user.AvatarJson = null;
                                break;
                        }


                        cacheKey = typeof(ApplicationUser).ToString() + ".Single." + model.Id;
                        _cache.Remove(cacheKey);
                        await _db.SaveChangesAsync();
                        return new Response(true, MediaObject.Blank, $"The media file has been removed successfully.");

                    case nameof(PropertyListing):

                        int propertyId = int.Parse(model.Id);
                        PropertyListing property = await _db.Properties.Where(p => p.Id == propertyId).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case nameof(PropertyListing.FeaturedImage):
                                property.FeaturedImageJson = null;
                                break;
                            case nameof(PropertyListing.InfoDownload):
                                property.InfoDownloadJson = null;
                                break;
                        }

                        await _db.SaveChangesAsync();
                        cacheKey = typeof(PropertyListing).ToString() + ".Single." + propertyId;
                        _cache.Remove(cacheKey);
                        return new Response(true, MediaObject.Blank, $"The media file has been removed successfully.");

                    case nameof(Subscription):

                        // create the new media item for content =>
                        int subId = int.Parse(model.Id);
                        Subscription subscription = await _db.Subscriptions.Where(p => p.Id == subId).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case nameof(Subscription.FeaturedImage):
                                subscription.FeaturedImage = null;
                                break;
                        }

                        return new Response(true, MediaObject.Blank, $"The media file has been removed successfully.");

                    case nameof(SubscriptionProduct):

                        // create the new media item for content =>
                        int subgId = int.Parse(model.Id);
                        SubscriptionProduct subscriptionGroup = await _db.SubscriptionProducts.Where(p => p.Id == subgId).FirstOrDefaultAsync();

                        switch (model.Field)
                        {
                            case nameof(SubscriptionProduct.FeaturedImage):
                                subscriptionGroup.FeaturedImage = null;
                                break;
                        }

                        return new Response(true, MediaObject.Blank, $"The media file has been removed successfully.");

                    case nameof(ContentMeta):

                        int idForMeta = int.Parse(model.Id);
                        Content contentForMeta = await _db.Content.Include(c => c.Metadata).Where(p => p.Id == idForMeta).FirstOrDefaultAsync();
                        MediaObject mi = await _db.Media.Where(m => m.Id == model.MediaId).FirstOrDefaultAsync();
                        contentForMeta.UpdateMeta(model.Field, mi);
                        if (await _db.SaveChangesAsync() == 0)
                        {
                            throw new Exception("Could not update the database");
                        }

                        cacheKey = typeof(Content).ToString() + ".Single." + idForMeta;
                        _cache.Remove(cacheKey);
                        return new Response(true, MediaObject.Blank, $"The media file has been removed successfully.");

                    default:
                        throw new Exception($"No entity supplied to remove from.");
                }

            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<MediaController>($"Error removing a media file from an entity.", ex);
            }
        }
        #endregion

        #region Uploads 
        [HttpPost]
        [Route("admin/media/upload/simple/")]
        public async Task<Response> UploadToDirectory(IEnumerable<IFormFile> files, int? directoryId)
        {
            try
            {
                if (!directoryId.HasValue)
                {
                    throw new Exception("You must select a directory to upload to.");
                }

                MediaDirectory directory = await _db.MediaDirectories.SingleOrDefaultAsync(md => md.Id == directoryId.Value);
                if (directory == null)
                {
                    throw new Exception($"The directory with id {directoryId} could not be found.");
                }

                if (directory.OwnerId != User.GetUserId())
                {
                    if (User.IsAdminOrBetter())
                    {
                        // admins can upload anywhere, let them through.
                    }
                    else if (User.IsEditorOrBetter())
                    {
                        // if the user is an editor, they can only upload to directories in the media folder.
                        MediaDirectory root = _directoryManager.GetTopLevelDirectory(directory.Id);
                        if (root.Type != DirectoryType.System || root.Slug != MediaManager.SiteDirectorySlug)
                        {
                            throw new Exception("You do not have permission to upload to this directory.");
                        }
                    }
                    else // otherwise throw
                    {
                        throw new Exception("You do not have permission to upload to this directory.");
                    }
                }

                if (files != null)
                {
                    foreach (IFormFile file in files)
                    {
                        MediaObject mediaResult = await _media.ProcessUpload(file, _directoryManager.GetPath(directory.Id)) as MediaObject;
                        mediaResult.DirectoryId = directoryId;
                        _db.Media.Add(mediaResult);
                        _db.SaveChanges();
                    }
                    _directoryManager.ResetCache();
                    return new Response(true, MediaObject.Blank, "The files have been uploaded successfully.");
                }
                else
                {
                    throw new Exception("No files supplied.");
                }
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<MediaController>($"Error uploading multiple files.", ex);
            }
        }
        #endregion
    }
}
