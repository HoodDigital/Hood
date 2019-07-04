using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin,Editor")]
    public class MediaController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        public MediaController()
            : base()
        {
        }

        [Route("admin/media/")]
        public async Task<IActionResult> Index()
        {
            ManageMediaModel model = new ManageMediaModel()
            {
                Directories = await _db.Media.Select(u => u.Directory).Distinct().ToListAsync()
            };
            if (!model.Directories.Contains("Default"))
                model.Directories.Add("Default");
            return View(model);
        }

        [Route("admin/media/directories/")]
        public async Task<IActionResult> Directories()
        {
            ManageMediaModel model = new ManageMediaModel()
            {
                Directories = await _db.Media.Select(u => u.Directory).Distinct().ToListAsync()
            };
            if (!model.Directories.Contains("Default"))
                model.Directories.Add("Default");
            return View(model);
        }

        [HttpGet]
        [Route("admin/media/get/")]
        public async Task<JsonResult> Get(ListFilters request, string search, string sort, string type, string directory, string container = "mediaitem")
        {
            IList<MediaObject> media = new List<MediaObject>();
            if (!string.IsNullOrEmpty(type))
            {
                media = await _db.Media.Where(m => m.GeneralFileType == type).ToListAsync();
            }
            else
            {
                media = await _db.Media.Where(m => m.GeneralFileType != GenericFileType.Directory.ToString()).ToListAsync();
            }
            if (!string.IsNullOrEmpty(directory))
            {
                media = media.Where(n => n.Directory == directory).ToList();
            }
            if (!string.IsNullOrEmpty(search))
            {
                string[] searchTerms = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                media = media.Where(n => searchTerms.Any(s => n.Filename.ToLower().Contains(s.ToLower()))).ToList();
            }
            switch (sort)
            {
                case "Size":
                    media = media.OrderBy(n => n.FileSize).ToList();
                    break;
                case "Filename":
                    media = media.OrderBy(n => n.Filename).ToList();
                    break;
                case "Date":
                    media = media.OrderBy(n => n.CreatedOn).ToList();
                    break;

                case "SizeDesc":
                    media = media.OrderByDescending(n => n.FileSize).ToList();
                    break;
                case "FilenameDesc":
                    media = media.OrderByDescending(n => n.Filename).ToList();
                    break;
                case "DateDesc":
                    media = media.OrderByDescending(n => n.CreatedOn).ToList();
                    break;

                default:
                    media = media.OrderByDescending(n => n.CreatedOn).ToList();
                    break;
            }
            Response response = new Response(media.Skip(request.skip).Take(request.take).ToArray(), media.Count());
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            return Json(response, settings);
        }

        [HttpGet]
        [Route("admin/media/getdirectories/")]
        public async Task<IEnumerable<string>> GetDirectories(bool normalised = false)
        {
            List<string> dirs = await _db.Media.Select(u => u.Directory).Distinct().OrderBy(s => s).ToListAsync();
            if (!dirs.Contains("Default"))
                dirs.Add("Default");
            return dirs;
        }

        [Route("admin/media/adddirectory/")]
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
                    GeneralFileType = GenericFileType.Directory.ToString(),
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

        [HttpGet]
        [Route("admin/media/getbyid/")]
        public async Task<JsonResult> GetById(int id)
        {
            IList<MediaObject> media = await _db.Media.Where(u => u.Id == id).ToListAsync();
            return Json(media.ToArray());
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
                return new Response(true);
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
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
                return new Response(true);
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }


        [Route("admin/media/blade/{id}/")]
        public async Task<IActionResult> Blade(int id)
        {
            MediaObject media = await _db.Media.SingleAsync(m => m.Id == id);
            return View(media);
        }

        // THIS HANDLES THE CHOOSE-ATTACH FUNCTION ON THE UPLOADER
        [Route("admin/media/attach/")]
        public async Task<IActionResult> AttachLoad(AttachMediaModel attach)
        {
            attach.Directories = await _db.Media.Select(u => u.Directory).Distinct().ToListAsync();
            if (!attach.Directories.Contains("Default"))
                attach.Directories.Add("Default");
            return View(attach);
        }

        [HttpPost]
        [Route("admin/media/attach/")]
        public async Task<MediaResponse> Attach(AttachMediaModel attach)
        {
            try
            {
                // load the media object.
                MediaObject media = _db.Media.SingleOrDefault(m => m.Id == attach.MediaId);
                string cacheKey;
                switch (attach.Entity)
                {
                    case "Content":

                        // create the new media item for content =>
                        int contentId = int.Parse(attach.Id);
                        Content content = await _db.Content.Where(p => p.Id == contentId).FirstOrDefaultAsync();

                        switch (attach.Field)
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
                        return new MediaResponse(true, media);

                    case "Forum":

                        // create the new media item for content =>
                        int forumId = int.Parse(attach.Id);
                        Forum forum = await _db.Forums.Where(p => p.Id == forumId).FirstOrDefaultAsync();

                        switch (attach.Field)
                        {
                            case "FeaturedImage":
                                forum.FeaturedImage = new ContentMedia(media);
                                break;
                            case "ShareImage":
                                forum.ShareImage = new ContentMedia(media);
                                break;
                        }

                        await _db.SaveChangesAsync();
                        return new MediaResponse(true, media);

                    case "ApplicationUser":

                        // create the new media item for content =>
                        ApplicationUser user = await _db.Users.Where(p => p.Id == attach.Id).FirstOrDefaultAsync();

                        switch (attach.Field)
                        {
                            case "Avatar":
                                user.Avatar = media;
                                break;
                        }


                        cacheKey = typeof(ApplicationUser).ToString() + ".Single." + attach.Id;
                        _cache.Remove(cacheKey);
                        await _db.SaveChangesAsync();
                        return new MediaResponse(true, media);

                    case "PropertyListing":

                        int propertyId = int.Parse(attach.Id);
                        PropertyListing property = await _db.Properties.Where(p => p.Id == propertyId).FirstOrDefaultAsync();

                        switch (attach.Field)
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
                        return new MediaResponse(true, media);

                    case "ContentMeta":

                        int idForMeta = int.Parse(attach.Id);
                        Content contentForMeta = await _db.Content.Include(c => c.Metadata).Where(p => p.Id == idForMeta).FirstOrDefaultAsync();
                        MediaObject mi = await _db.Media.Where(m => m.Id == attach.MediaId).FirstOrDefaultAsync();
                        contentForMeta.UpdateMeta(attach.Field, mi);
                        if (await _db.SaveChangesAsync() == 0)
                            throw new Exception("Could not update the database");
                        cacheKey = typeof(Content).ToString() + ".Single." + idForMeta;
                        _cache.Remove(cacheKey);
                        return new MediaResponse(true, media);

                    default:
                        return new MediaResponse(false);
                }

            }
            catch (Exception ex)
            {
                return new MediaResponse(ex.Message);
            }
        }

        // THIS HANDLES THE INSERT TO EDITOR
        [Route("admin/media/insert/")]
        public async Task<IActionResult> Insert(AttachMediaModel attach)
        {
            attach.Directories = await _db.Media.Select(u => u.Directory).Distinct().ToListAsync();
            if (!attach.Directories.Contains("Default"))
                attach.Directories.Add("Default");
            return View(attach);
        }

        // THIS HANDLES THE SWITCH/SET FUNCTIONS ON THE UPLOADER
        [Route("admin/media/select/")]
        public async Task<IActionResult> Select(AttachMediaModel attach)
        {
            attach.Directories = await _db.Media.Select(u => u.Directory).Distinct().ToListAsync();
            if (!attach.Directories.Contains("Default"))
                attach.Directories.Add("Default");
            return View(attach);
        }

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
                return Json(new { Success = false, Message = ex.Message });
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
                return Json(new { Success = false, Message = ex.Message });
            }
        }
    }
}
