﻿using Geocoding.Google;
using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class PropertyController : BaseController
    {
        protected readonly PropertySettings _propertySettings;

        public PropertyController()
            : base()
        {
            _propertySettings = Engine.Settings.Property;
        }

        [Route("admin/property/manage/")]
        public async Task<IActionResult> Index(PropertyListModel model)
        {
            return await List(model, "Index");
        }

        [Route("admin/property/list/")]
        public async Task<IActionResult> List(PropertyListModel model, string viewName = "_List_Property")
        {
            PropertySettings propertySettings = Engine.Settings.Property;
            if (!propertySettings.Enabled || !propertySettings.ShowList)
            {
                return NotFound();
            }
            model = await _property.GetPropertiesAsync(model);
            return View(viewName, model);
        }

        #region Edit
        [Route("admin/property/edit/{id}/")]
        public async Task<IActionResult> Edit(int id)
        {
            PropertyListing model = await _property.GetPropertyByIdAsync(id, true);
            model = await LoadAgents(model);
            model.AutoGeocode = true;
            return View(model);
        }

        [HttpPost]
        [Route("admin/property/edit/{id}/")]
        public async Task<ActionResult> Edit(PropertyListing model)
        {
            try
            {
                model = await _property.ReloadReferences(model);

                model.PublishDate = model.PublishDate.AddHours(model.PublishHour);
                model.PublishDate = model.PublishDate.AddMinutes(model.PublishMinute);
                model.LastEditedBy = User.Identity.Name;
                model.LastEditedOn = DateTime.Now;

                if (model.AutoGeocode)
                {
                    var StatusMessage = "";
                    try
                    {

                        try
                        {
                            GoogleAddress address = _address.GeocodeAddress(model);
                            if (address != null)
                            {
                                model.SetLocation(address.Coordinates);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.InnerException != null)
                            {
                                throw ex.InnerException;
                            }
                            StatusMessage = "There was an error GeoLocating the property.";
                            ModelState.AddModelError("GeoCoding", StatusMessage);
                            await _logService.AddExceptionAsync<PropertyController>(StatusMessage, ex, LogType.Warning);
                        }
                    }
                    catch (GoogleGeocodingException ex)
                    {
                        switch (ex.Status)
                        {
                            case GoogleStatus.RequestDenied:
                                StatusMessage = "There was an error with the Google API [RequestDenied] this means your API account is not activated for Geocoding Requests.";
                                ModelState.AddModelError("GeoCoding", StatusMessage);
                                await _logService.AddExceptionAsync<PropertyController>(StatusMessage, ex, LogType.Warning);
                                break;
                            case GoogleStatus.OverQueryLimit:
                                StatusMessage = "There was an error with the Google API [OverQueryLimit] this means your API account is has run out of Geocoding Requests.";
                                ModelState.AddModelError("GeoCoding", StatusMessage);
                                await _logService.AddExceptionAsync<PropertyController>(StatusMessage, ex, LogType.Warning);
                                break;
                            default:
                                StatusMessage = "There was an error with the Google API [" + ex.Status.ToString() + "]: " + ex.Message;
                                ModelState.AddModelError("GeoCoding", StatusMessage);
                                await _logService.AddExceptionAsync<PropertyController>(StatusMessage, ex, LogType.Warning);
                                break;
                        }
                    }

                }

                string type = Engine.Settings.Property.GetPlanningFromType(model.Planning);
                if (model.HasMeta("PlanningDescription"))
                {
                    model.UpdateMeta("PlanningDescription", type);
                }
                else
                {
                    if (model.Metadata == null)
                    {
                        model.Metadata = new List<PropertyMeta>();
                    }

                    model.AddMeta("PlanningDescription", type);
                }

                foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> val in Request.Form)
                {
                    if (val.Key.StartsWith("Meta:"))
                    {
                        if (model.HasMeta(val.Key.Replace("Meta:", "")))
                        {
                            model.UpdateMeta(val.Key.Replace("Meta:", ""), val.Value.ToString());
                        }
                        else
                        {
                            model.AddMeta(val.Key.Replace("Meta:", ""), val.Value.ToString());
                        }
                    }
                }

                await _property.UpdateAsync(model);

                SaveMessage = "Saved";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                await _logService.AddExceptionAsync<PropertyController>("Error saving a property listing.", ex);

                SaveMessage = "An error occurred: " + ex.Message;
                MessageType = AlertType.Danger;
            }

            model = await LoadAgents(model);

            return View(model);
        }
        private async Task<PropertyListing> LoadAgents(PropertyListing listing)
        {
            IList<ApplicationUser> admins = await _userManager.GetUsersInRoleAsync("Admin");
            IList<ApplicationUser> editors = await _userManager.GetUsersInRoleAsync("Editor");
            listing.AvailableAgents = editors.Concat(admins).Distinct().OrderBy(u => u.FirstName).ThenBy(u => u.Email).ToList();
            return listing;
        }

        #endregion

        #region Create
        [Route("admin/property/create/")]
        public IActionResult Create()
        {
            PropertyListing model = new PropertyListing()
            {
                AgentId = User.GetUserId()
            };
            return View("_Blade_Property", model);
        }
        [HttpPost]
        [Route("admin/property/create/")]
        public async Task<Response> Create(PropertyListing model)
        {
            try
            {
                model.AgentId = Engine.Account.Id;
                model.CreatedBy = Engine.Account.UserName;
                model.CreatedOn = DateTime.Now;
                model.LastEditedBy = Engine.Account.UserName;
                model.LastEditedOn = DateTime.Now;
                model.Confidential = false;
                model.Featured = false;
                model.AskingPrice = 0;
                model.Fees = 0;
                model.Rent = 0;
                model.Premium = 0;
                model.AskingPriceDisplay = "{0}";
                model.FeesDisplay = "{0}";
                model.RentDisplay = "{0}";
                model.PremiumDisplay = "{0}";
                model.ShareCount = 0;
                model.Views = 0;

                List<string> leaseStatuses = _propertySettings.GetLeaseStatuses();
                if (leaseStatuses.Count > 0)
                {
                    model.LeaseStatus = leaseStatuses.FirstOrDefault();
                }
                else
                {
                    model.LeaseStatus = "Available";
                }

                Dictionary<string, string> planningTypes = _propertySettings.GetPlanningTypes();
                if (planningTypes.Count > 0)
                {
                    model.Planning = planningTypes.FirstOrDefault().Key;
                }
                else
                {
                    model.Planning = "VAR";
                }

                List<string> listingTypes = _propertySettings.GetListingTypes();
                if (listingTypes.Count > 0)
                {
                    model.ListingType = listingTypes.FirstOrDefault();
                }
                else
                {
                    model.ListingType = "Not Specified";
                }

                // Geocode
                Geocoding.Google.GoogleAddress address = _address.GeocodeAddress(model);
                if (address != null)
                {
                    model.SetLocation(address.Coordinates);
                }

                await _property.AddAsync(model);
                if (model.Metadata == null)
                {
                    model.Metadata = new List<PropertyMeta>();
                }

                model.UpdateMeta("PlanningDescription", Engine.Settings.Property.GetPlanningTypes().FirstOrDefault().Value);

                for (int i = 0; i < 11; i++)
                {
                    model.AddMeta("Feature" + i.ToString(), "", "System.String");
                }

                await _property.UpdateAsync(model);
                return new Response(true, "Created successfully."); ;
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error creating a new property.", ex);
            }
        }

        #endregion

        #region Delete
        [Authorize(Roles = "SuperUser,Admin")]
        [HttpPost()]
        [Route("admin/property/delete/all/")]
        public async Task<Response> DeleteAll()
        {
            try
            {
                await _property.DeleteAllAsync();
                return new Response(true, "The properties have been successfully deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error deleting all properties.", ex);
            }
        }

        [Route("admin/property/delete/{id}")]
        [HttpPost()]
        public async Task<Response> Delete(int id)
        {
            try
            {
                await _property.DeleteAsync(id);
                return new Response(true, "The property has been successfully deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error deleting a property.", ex);
            }
        }
        #endregion

        [Route("admin/property/set-status/{id}")]
        [HttpPost()]
        public async Task<Response> SetStatus(int id, ContentStatus status)
        {
            try
            {
                await _property.SetStatusAsync(id, status);
                return new Response(true, "Property status has been updated successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ContentController>($"Error publishing/archiving property with Id: {id}", ex);
            }
        }

        #region Gallery
        [Route("admin/property/gallery/{id}/")]
        public async Task<IActionResult> Gallery(int id)
        {
            PropertyListing model = await _property.GetPropertyByIdAsync(id, true);
            return View("_List_PropertyMedia", model);
        }

        [Authorize]
        [Route("admin/property/upload/gallery")]
        public async Task<Response> UploadToGallery(List<IFormFile> files, int id)
        {
            try
            {
                PropertyListing property = await _property.GetPropertyByIdAsync(id);
                if (property == null)
                {
                    throw new Exception("Property not found!");
                }

                MediaObject mediaResult = null;
                if (files != null)
                {
                    if (files.Count == 0)
                    {
                        throw new Exception("There are no files attached!");
                    }

                    var directory = await _property.GetDirectoryAsync();
                    foreach (IFormFile file in files)
                    {
                        mediaResult = await _media.ProcessUpload(file, _directoryManager.GetPath(directory.Id)) as MediaObject;
                        await _property.AddMediaAsync(property, new PropertyMedia(mediaResult));
                        mediaResult.DirectoryId = directory.Id;
                        _db.Media.Add(mediaResult);
                        await _db.SaveChangesAsync();
                    }
                }
                return new Response(true, mediaResult, "The image/media file has been attached successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error uploading media to the gallery.", ex);
            }
        }

        [HttpGet]
        [Route("admin/property/media/setfeatured/{id}/{mediaId}")]
        public async Task<Response> SetFeatured(int id, int mediaId)
        {
            try
            {
                PropertyListing property = await _property.GetPropertyByIdAsync(id, true);
                PropertyMedia media = property.Media.SingleOrDefault(m => m.Id == mediaId);
                if (media != null)
                {
                    property.FeaturedImage = new MediaObject(media);
                    await _property.UpdateAsync(property);
                }
                return new Response(true, $"Featured image has been updated. You may need to reload the page to see the change.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error setting featured image for property.", ex);
            }
        }

        [HttpPost]
        [Route("admin/property/media/remove/{id}/{mediaId}")]
        public async Task<Response> RemoveMedia(int id, int mediaId)
        {
            try
            {
                PropertyListing model = await _property.GetPropertyByIdAsync(id, true);
                PropertyMedia media = model.Media.Find(m => m.Id == mediaId);
                if (media != null)
                {
                    var mediaItem = await _db.Media.SingleOrDefaultAsync(m => m.UniqueId == media.UniqueId);
                    if (mediaItem != null)
                    {
                        _db.Entry(mediaItem).State = EntityState.Deleted;
                    }
                    await _media.DeleteStoredMedia(media);
                    _db.Entry(media).State = EntityState.Deleted;
                }
                await _property.UpdateAsync(model);
                return new Response(true, $"The media has been removed.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error removing media from property.", ex);
            }
        }

        #endregion

        #region Floorplans
        [Route("admin/property/floorplans/{id}/")]
        public async Task<IActionResult> FloorPlans(int id)
        {
            PropertyListing model = await _property.GetPropertyByIdAsync(id, true);
            return View("_List_PropertyFloorplans", model);
        }

        [Authorize]
        [Route("admin/property/upload/floorplan")]
        public async Task<Response> UploadFloorplan(List<IFormFile> files, int id)
        {
            try
            {
                PropertyListing property = await _property.GetPropertyByIdAsync(id);
                if (property == null)
                {
                    throw new Exception("Property not found!");
                }

                MediaObject mediaResult = null;
                if (files != null)
                {
                    if (files.Count == 0)
                    {
                        throw new Exception("There are no files attached!");
                    }

                    var directory = await _property.GetDirectoryAsync();
                    foreach (IFormFile file in files)
                    {
                        mediaResult = await _media.ProcessUpload(file, _directoryManager.GetPath(directory.Id)) as MediaObject;
                        await _property.AddFloorplanAsync(property, new PropertyFloorplan(mediaResult));
                        mediaResult.DirectoryId = directory.Id;
                        _db.Media.Add(mediaResult);
                        await _db.SaveChangesAsync();
                    }
                }
                return new Response(true, mediaResult, "The floorplan file has been attached successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error uploading a floorplan.", ex);
            }
        }
        [HttpPost]
        [Route("admin/property/floorplan/remove/{id}/{mediaId}")]
        public async Task<Response> RemoveFloorplan(int id, int mediaId)
        {
            try
            {
                PropertyListing model = await _property.GetPropertyByIdAsync(id, true);
                PropertyFloorplan media = model.FloorPlans.Find(m => m.Id == mediaId);
                if (media != null)
                {
                    var mediaItem = await _db.Media.SingleOrDefaultAsync(m => m.UniqueId == media.UniqueId);
                    if (mediaItem != null)
                    {
                        _db.Entry(mediaItem).State = EntityState.Deleted;
                    }
                    await _media.DeleteStoredMedia(media);
                    _db.Entry(media).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }
                await _property.UpdateAsync(model);
                return new Response(true, $"The floorplan has been removed.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error removing floorplan from property.", ex);
            }
        }
        #endregion

        #region Floor Areas
        [Route("admin/property/floorareas/{id}/")]
        public async Task<IActionResult> FloorAreas(int id)
        {
            PropertyListing model = await _property.GetPropertyByIdAsync(id, true);
            return View("_List_PropertyFloorareas", model);
        }

        [Authorize]
        [Route("admin/property/floorareas/create/{id}")]
        public async Task<IActionResult> CreateFloorArea(int id)
        {
            PropertyListing property = await _property.GetPropertyByIdAsync(id, true);
            if (property == null)
            {
                throw new Exception("Property not found!");
            }
            var model = new FloorArea()
            {
                PropertyId = property.Id,
                Number = property.FloorAreas.Count + 1
            };
            return View("_Blade_FloorArea", model);
        }

        [Authorize]
        [HttpPost]
        [Route("admin/property/floorareas/create/{id}")]
        public async Task<Response> CreateFloorArea(FloorArea model)
        {
            try
            {
                PropertyListing property = await _property.GetPropertyByIdAsync(model.PropertyId);
                if (property == null)
                {
                    throw new Exception("Property not found!");
                }

                var floorArea = property.FloorAreas.Find(m => m.Number == model.Number);
                if (floorArea != null)
                    throw new Exception("You cannot add multiple floor areas with the same floor number.");
                else
                {
                    var fa = property.FloorAreas;
                    fa.Add(model);
                    property.FloorAreas = fa;
                }

                await _property.UpdateAsync(property);
                return new Response(true, "Floor area added successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error uploading a floor area.", ex);
            }
        }
        [HttpPost]
        [Route("admin/property/floorplan/remove/{id}/{number}")]
        public async Task<Response> RemoveFloorArea(int id, int number)
        {
            try
            {
                PropertyListing property = await _property.GetPropertyByIdAsync(id, true);
                if (property == null)
                {
                    throw new Exception("Property not found!");
                }

                var floorArea = property.FloorAreas.Find(m => m.Number == number);
                if (floorArea != null)
                {
                    var fa = property.FloorAreas;
                    fa.Remove(floorArea);
                    property.FloorAreas = fa;
                }

                await _property.UpdateAsync(property);
                return new Response(true, $"The floorplan has been removed.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error removing floorplan from property.", ex);
            }
        }
        #endregion

        #region Features
        [Route("admin/property/add-meta/{id}/")]
        public async Task<IActionResult> AddMeta(int id, string name)
        {
            try
            {
                PropertyListing property = await _db.Properties.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
                if (property == null)
                {
                    throw new Exception("Property not found.");
                }

                int? count = await _db.PropertyMetadata.Where(m => m.Name.Contains($"{name}")).CountAsync();
                if (!count.HasValue)
                {
                    count = 0;
                }

                PropertyMeta meta = new PropertyMeta()
                {
                    PropertyId = property.Id,
                    Name = name,
                    Type = "System.String"
                };
                meta.SetValue("");
                _db.Add(meta);
                await _db.SaveChangesAsync();
                SaveMessage = $"Successfully added new field: {meta.Name}.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error adding a property meta: {name}.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<PropertyController>(SaveMessage, ex);
            }

            return RedirectToAction(nameof(Edit), new { id });
        }
        [Route("admin/property/delete-meta/{id}/")]
        public async Task<IActionResult> DeleteMeta(int id, int metaId)
        {
            try
            {
                PropertyMeta meta = await _db.PropertyMetadata.FindAsync(metaId);
                _db.Entry(meta).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                SaveMessage = $"Successfully deleted {meta.Name}.";
                MessageType = AlertType.Success;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error deleting a property meta.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<PropertyController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Edit), new { id });
        }
        #endregion

    }
}


