﻿using Hood.BaseControllers;
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

namespace Hood.Admin.BaseControllers
{
    public abstract class BaseMediaController : BaseController
    {
        public BaseMediaController()
            : base()
        {
        }

        [Route("admin/media/list/")]
        public virtual async Task<IActionResult> List(MediaListModel model, string viewName = "_List_Media")
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
                media = media.Where(m => m.DirectoryId == model.DirectoryId);
            }
            else
            {
                IEnumerable<MediaDirectory> userDirs = GetDirectoriesForCurrentUser();
                IEnumerable<int> tree = _directoryManager.GetAllCategoriesIncludingChildren(userDirs).Select(d => d.Id);
                media = media.Where(n => n.DirectoryId.HasValue && tree.Contains(n.DirectoryId.Value));
            }

            if (model.UserId.IsSet())
            {
                media = media.Where(n => n.Directory.OwnerId == model.UserId);
            }

            if (model.Search.IsSet())
            {
                media = media.Where(m =>
                  m.Filename.Contains(model.Search) ||
                  m.FileType.Contains(model.Search)
              );
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
        public virtual async Task<IActionResult> Index(MediaListModel model)
        {
            return await List(model, "Index");
        }

        [Route("admin/media/action/")]
        public virtual async Task<IActionResult> Action(MediaListModel model)
        {
            return await List(model, "Action");
        }

        [Route("admin/media/blade/")]
        public virtual async Task<IActionResult> Blade(int id)
        {
            MediaObject media = await _db.Media.Include(m => m.Directory).SingleOrDefaultAsync(u => u.Id == id);
            return View("_Blade_Media", media);
        }

        [HttpPost]
        [Route("admin/media/delete/")]
        public virtual async Task<Response> Delete(int id)
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
                return await ErrorResponseAsync<BaseMediaController>($"Error deleting a media object.", ex);
            }
        }

        #region Directories
        [Route("admin/media/directories/list/")]
        public virtual IActionResult Directories(MediaListModel model)
        {
            model.TopLevelDirectories = GetDirectoriesForCurrentUser();
            return View("_List_Directories", model);
        }

        protected virtual IEnumerable<MediaDirectory> GetDirectoriesForCurrentUser()
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
                return _directoryManager.UserDirectories(User.GetLocalUserId());
            }
        }

        [Route("admin/media/directory/create/")]
        public virtual async Task<IActionResult> CreateDirectory(int id)
        {
            try
            {
                MediaDirectory parentDirectory = await _db.MediaDirectories.SingleOrDefaultAsync(md => md.Id == id);
                if (parentDirectory == null)
                {
                    throw new Exception($"The parent directory could not be found.");
                }

                MediaDirectory root = _directoryManager.GetTopLevelDirectory(parentDirectory.Id);

                if (!User.IsEditorOrBetter())
                {
                    if (root.Type != DirectoryType.System || root.Slug != MediaManager.UserDirectorySlug)
                    {
                        throw new Exception("You cannot create directories outside the user folders folders.");
                    }

                    if (parentDirectory.OwnerId != User.GetLocalUserId())
                    {
                        throw new Exception("You cannot create directories outside your own folder.");
                    }
                }

                MediaDirectory model = new MediaDirectory
                {
                    TopLevelDirectories = GetDirectoriesForCurrentUser(),
                    ParentId = id
                };
                return View("_Blade_Directory", model);

            }
            catch (Exception ex)
            {
                return View("_Blade_DirectoryDenied", ex);
            }
        }

        [HttpPost]
        [Route("admin/media/directory/create/")]
        public virtual async Task<Response> CreateDirectory(MediaDirectory model)
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
                if (!User.IsEditorOrBetter() && root.Slug != MediaManager.UserDirectorySlug)
                {
                    throw new Exception("You cannot create directories outside the user folders folders.");
                }

                if (!User.IsEditorOrBetter() && parentDirectory.OwnerId != User.GetLocalUserId())
                {
                    throw new Exception("You cannot create directories outside your own folder.");
                }

                model.OwnerId = User.GetLocalUserId();
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
        public virtual async Task<Response> DeleteDirectory(int id)
        {
            try
            {
                MediaDirectory directory = await _db.MediaDirectories
                    .Include(md => md.Children)
                    .Include(md => md.Media)
                    .SingleOrDefaultAsync(md => md.Id == id);

                int? parent = directory.ParentId;

                if (directory == null)
                {
                    throw new Exception("You have to select a directory to delete.");
                }

                if (directory.Type == DirectoryType.System)
                {
                    throw new Exception("You cannot delete a system directory.");
                }

                if (directory.Type == DirectoryType.User)
                {
                    throw new Exception("You cannot delete a user directory.");
                }

                if (directory.Children.Count > 0 || directory.Media.Count > 0)
                {
                    throw new Exception("You cannot delete a non-empty directory.");
                }

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
                Response response = new Response(true, $"The directory has been deleted.");
                if (parent.HasValue)
                {
                    // return the directory parent id so the list can refresh correctly.
                    response.Data = (new List<int>() { parent.Value }).ToArray();
                }
                return response;
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BaseMediaController>(ex.Message, ex);
            }
        }
        #endregion

        #region Uploads 
        [HttpPost]
        [Route("admin/media/upload/simple/")]
        public virtual async Task<Response> UploadToDirectory(IEnumerable<IFormFile> files, int? directoryId)
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

                if (directory.OwnerId != User.GetLocalUserId())
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
                return await ErrorResponseAsync<BaseMediaController>($"Error uploading multiple files.", ex);
            }
        }
        #endregion
    }
}
