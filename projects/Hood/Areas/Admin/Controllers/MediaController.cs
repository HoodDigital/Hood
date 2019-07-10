using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
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

        [Route("admin/media/")]
        public async Task<IActionResult> Index(MediaListModel model) => await List(model, "Index");

        [HttpGet]
        [Route("admin/media/list/")]
        public async Task<IActionResult> List(MediaListModel model, string viewName = "_List_Media")
        {
            var media = _db.Media.AsQueryable();

            if (model.GenericFileType.HasValue)
            {
                media = media.Where(m => m.GenericFileType == model.GenericFileType);
            }
            else
            {
                media = media.Where(m => m.GenericFileType != GenericFileType.Directory);
            }

            if (model.Directory.IsSet())
            {
                media = media.Where(n => n.Directory == model.Directory);
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

            model.Directories = await GetDirectories();

            return View(viewName, model);
        }

        [HttpGet]
        [Route("admin/media/action/")]
        public async Task<IActionResult> Action(MediaListModel model) => await List(model, "Action");

        [HttpGet]
        [Route("admin/media/blade/")]
        public async Task<IActionResult> Blade(int id)
        {
            MediaObject media = await _db.Media.SingleOrDefaultAsync(u => u.Id == id);
            return View("_Blade_Media", media);
        }

        [HttpPost()]
        [Route("admin/media/delete/")]
        public async Task<Response> Delete(int id)
        {
            try
            {
                var media = _db.Media.SingleOrDefault(m => m.Id == id);
                try { await _media.DeleteStoredMedia(media); } catch (Exception) { }
                _db.Media.Remove(media);
                await _db.SaveChangesAsync();
                return new Response(true, $"The media has been deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<MediaController>($"Error deleting a media object.", ex);
            }
        }

        [HttpGet]
        [Route("admin/media/getdirectories/")]
        public async Task<List<string>> GetDirectories()
        {
            List<string> dirs = await _db.Media.Select(u => u.Directory).Distinct().OrderBy(s => s).ToListAsync();
            if (!dirs.Contains("Default"))
                dirs.Add("Default");
            return dirs;
        }

        [Route("admin/media/directory/add/")]
        public async Task<Response> AddDirectory(string directory)
        {
            try
            {
                MediaObject newMedia = new MediaObject()
                {
                    BlobReference = "",
                    CreatedOn = DateTime.Now,
                    Filename = directory,
                    FileSize = 0,
                    FileType = "directory/dir",
                    GenericFileType = GenericFileType.Directory,
                    SmallUrl = "",
                    MediumUrl = "",
                    LargeUrl = "",
                    Directory = directory,
                    Url = ""
                };
                _db.Media.Add(newMedia);
                await _db.SaveChangesAsync();
                return new Models.Response(true);
            }
            catch (Exception ex)
            {
                return new Models.Response(ex.Message);
            }

        }

        [HttpPost()]
        [Route("admin/media/directory/delete")]
        public async Task<Response> DeleteDirectory(string directory)
        {
            try
            {
                var directoryList = _db.Media.AsQueryable();
                if (directory.IsSet())
                    directoryList = directoryList.Where(m => m.Directory == directory);

                foreach (var media in directoryList)
                {
                    try { await _media.DeleteStoredMedia(media); } catch (Exception) { }
                    _db.Entry(media).State = EntityState.Deleted;
                }

                await _db.SaveChangesAsync();
                return new Response(true, $"The directory has been deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<MediaController>($"Error deleting a directory.", ex);
            }
        }

        /// <summary>
        /// This is the action which handles the chosen attachment from the CHOOSE -> ATTACH modal.
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

                        await _db.SaveChangesAsync();
                        cacheKey = typeof(Content).ToString() + ".Single." + contentId;
                        _cache.Remove(cacheKey);
                        return new Response(true, media);

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

                        await _db.SaveChangesAsync();
                        return new Response(true, media);

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
                        await _db.SaveChangesAsync();
                        return new Response(true, media);

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

                        await _db.SaveChangesAsync();
                        cacheKey = typeof(PropertyListing).ToString() + ".Single." + propertyId;
                        _cache.Remove(cacheKey);
                        return new Response(true, media);

                    case "ContentMeta":

                        int idForMeta = int.Parse(model.Id);
                        Content contentForMeta = await _db.Content.Include(c => c.Metadata).Where(p => p.Id == idForMeta).FirstOrDefaultAsync();
                        MediaObject mi = await _db.Media.Where(m => m.Id == model.MediaId).FirstOrDefaultAsync();
                        contentForMeta.UpdateMeta(model.Field, mi);
                        if (await _db.SaveChangesAsync() == 0)
                            throw new Exception("Could not update the database");
                        cacheKey = typeof(Content).ToString() + ".Single." + idForMeta;
                        _cache.Remove(cacheKey);
                        return new Response(true, media);

                    default:
                        return new Response(false);
                }

            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<MediaController>($"Error attaching a media file to an entity.", ex);
            }
        }

        #region Uploads 

        [HttpPost]
        [Route("admin/media/upload/simple/")]
        public async Task<IActionResult> UploadToDirectory(IEnumerable<IFormFile> files, string directory)
        {
            try
            {
                if (!directory.IsSet())
                    directory = "Default";
                if (files != null)
                {
                    foreach (IFormFile file in files)
                    {
                        MediaObject mi = await _media.ProcessUpload(file, new MediaObject() { Directory = directory });
                        _db.Media.Add(mi);
                        _db.SaveChanges();
                    }
                    return Json(new { Success = true });
                }
                else
                    throw new Exception("No files supplied.");
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, ex.Message });
            }
        }

        [HttpPost]
        [Route("admin/media/upload/single/")]
        public async Task<IActionResult> UploadSingle(IFormFile file, string directory)
        {
            try
            {
                if (!directory.IsSet())
                    directory = "Default";
                if (file != null)
                {
                    MediaObject mi = await _media.ProcessUpload(file, new MediaObject() { Directory = directory });
                    _db.Media.Add(mi);
                    _db.SaveChanges();
                    return Json(new { Success = false, Message = mi.LargeUrl });
                }
                else
                    throw new Exception("No file supplied.");

            }
            catch (Exception ex)
            {
                return Json(new { Success = false, ex.Message });
            }
        }

        #endregion
    }
}
