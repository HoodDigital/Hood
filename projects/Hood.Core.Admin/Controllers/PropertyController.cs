using Geocoding.Google;
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
    public abstract class BasePropertyController : BaseController
    {
        protected readonly PropertySettings _propertySettings;

        public BasePropertyController()
            : base()
        {
            _propertySettings = Engine.Settings.Property;
        }

        [Route("admin/property/manage/")]
        public virtual async Task<IActionResult> Index(PropertyListModel model)
        {
            return await List(model, "Index");
        }

        [Route("admin/property/list/")]
        public virtual async Task<IActionResult> List(PropertyListModel model, string viewName = "_List_Property")
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
        [Route("admin/property/{id}/edit/")]
        public virtual async Task<IActionResult> Edit(int id)
        {
            PropertyListing model = await _property.GetPropertyByIdAsync(id, true);
            model = await LoadAgents(model);
            model.AutoGeocode = true;
            return View(model);
        }

        [HttpPost]
        [Route("admin/property/{id}/edit/")]
        public virtual async Task<ActionResult> Edit(PropertyListing model)
        {
            try
            {
                var modelToUpdate = await _property.GetPropertyByIdAsync(model.Id, true);

                var updatedFields = Request.Form.Keys.ToHashSet();
                modelToUpdate = modelToUpdate.UpdateFromFormModel(model, updatedFields);

                modelToUpdate = await _property.ReloadReferences(modelToUpdate);

                modelToUpdate.LastEditedBy = User.Identity.Name;
                modelToUpdate.LastEditedOn = DateTime.UtcNow;

                if (model.AutoGeocode)
                {
                    var StatusMessage = "";
                    try
                    {

                        try
                        {
                            GoogleAddress address = _address.GeocodeAddress(modelToUpdate);
                            if (address != null)
                            {
                                modelToUpdate.SetLocation(address.Coordinates);
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
                            await _logService.AddExceptionAsync<BasePropertyController>(StatusMessage, ex, LogType.Warning);
                        }
                    }
                    catch (GoogleGeocodingException ex)
                    {
                        switch (ex.Status)
                        {
                            case GoogleStatus.RequestDenied:
                                StatusMessage = "There was an error with the Google API [RequestDenied] this means your API account is not activated for Geocoding Requests.";
                                ModelState.AddModelError("GeoCoding", StatusMessage);
                                await _logService.AddExceptionAsync<BasePropertyController>(StatusMessage, ex, LogType.Warning);
                                break;
                            case GoogleStatus.OverQueryLimit:
                                StatusMessage = "There was an error with the Google API [OverQueryLimit] this means your API account is has run out of Geocoding Requests.";
                                ModelState.AddModelError("GeoCoding", StatusMessage);
                                await _logService.AddExceptionAsync<BasePropertyController>(StatusMessage, ex, LogType.Warning);
                                break;
                            default:
                                StatusMessage = "There was an error with the Google API [" + ex.Status.ToString() + "]: " + ex.Message;
                                ModelState.AddModelError("GeoCoding", StatusMessage);
                                await _logService.AddExceptionAsync<BasePropertyController>(StatusMessage, ex, LogType.Warning);
                                break;
                        }
                    }

                }

                string type = Engine.Settings.Property.GetPlanningFromType(modelToUpdate.Planning);
                if (modelToUpdate.HasMeta("PlanningDescription"))
                {
                    modelToUpdate.UpdateMeta("PlanningDescription", type);
                }
                else
                {
                    if (modelToUpdate.Metadata == null)
                    {
                        modelToUpdate.Metadata = new List<PropertyMeta>();
                    }

                    modelToUpdate.AddMeta("PlanningDescription", type);
                }

                foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> val in Request.Form)
                {
                    if (val.Key.StartsWith("Meta:"))
                    {
                        if (modelToUpdate.HasMeta(val.Key.Replace("Meta:", "")))
                        {
                            modelToUpdate.UpdateMeta(val.Key.Replace("Meta:", ""), val.Value.ToString());
                        }
                        else
                        {
                            modelToUpdate.AddMeta(val.Key.Replace("Meta:", ""), val.Value.ToString());
                        }
                    }
                }

                await _property.UpdateAsync(modelToUpdate);

                SaveMessage = "Saved";
                MessageType = AlertType.Success;

                modelToUpdate = await LoadAgents(modelToUpdate);

                return View(modelToUpdate);

            }
            catch (Exception ex)
            {
                await _logService.AddExceptionAsync<BasePropertyController>("Error saving a property listing.", ex);

                SaveMessage = "An error occurred: " + ex.Message;
                MessageType = AlertType.Danger;

                model = await _property.ReloadReferences(model);
                model = await LoadAgents(model);

                return View(model);
            }

        }
        protected virtual async Task<PropertyListing> LoadAgents(PropertyListing listing)
        {
            IList<ApplicationUser> admins = await _account.GetUsersInRole("Admin");
            IList<ApplicationUser> editors = await _account.GetUsersInRole("Editor");
            listing.AvailableAgents = editors.Concat(admins).Distinct().OrderBy(u => u.FirstName).ThenBy(u => u.Email).ToList();
            return listing;
        }

        #endregion

        #region Create
        [Route("admin/property/create/")]
        public virtual IActionResult Create()
        {
            PropertyListing model = new PropertyListing()
            {
                PublishDate = DateTime.UtcNow,
                AgentId = User.GetLocalUserId()
            };
            return View("_Blade_Property", model);
        }
        [HttpPost]
        [Route("admin/property/create/")]
        public virtual async Task<Response> Create(PropertyListing model)
        {
            try
            {
                model.AgentId = User.GetLocalUserId();
                model.CreatedBy = User.Identity.Name;
                model.CreatedOn = DateTime.UtcNow;
                model.LastEditedBy = User.Identity.Name;
                model.LastEditedOn = DateTime.UtcNow;
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
                return await ErrorResponseAsync<BasePropertyController>($"Error creating a new property.", ex);
            }
        }

        #endregion

        #region Delete
        [Route("admin/property/{id}/delete")]
        [HttpPost()]
        public virtual async Task<Response> Delete(int id)
        {
            try
            {
                await _property.DeleteAsync(id);
                return new Response(true, "The property has been successfully deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BasePropertyController>($"Error deleting a property.", ex);
            }
        }
        #endregion

        [Route("admin/property/{id}/set-status")]
        [HttpPost()]
        public virtual async Task<Response> SetStatus(int id, ContentStatus status)
        {
            try
            {
                await _property.SetStatusAsync(id, status);
                return new Response(true, "Property status has been updated successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BasePropertyController>($"Error publishing/archiving property with Id: {id}", ex);
            }
        }

        #region Media
        /// <summary>
        /// Attach media file to entity. This is the action which handles the chosen attachment from the media attach action.
        /// </summary>
        [HttpPost]
        [Route("admin/property/{id}/media/upload")]
        public virtual async Task<Response> UploadMedia(int id, AttachMediaModel model)
        {
            try
            {
                model.ValidateOrThrow();

                // load the media object.
                PropertyListing property = await _db.Properties.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (property == null)
                {
                    throw new Exception("Could not load property to attach media.");
                }

                MediaObject media = _db.Media.SingleOrDefault(m => m.Id == model.MediaId);
                if (media == null)
                {
                    throw new Exception("Could not load media to attach.");
                }

                switch (model.FieldName)
                {
                    case nameof(Models.PropertyListing.FeaturedImage):
                        property.FeaturedImage = new PropertyMedia(media);
                        break;
                    case nameof(Models.PropertyListing.InfoDownload):
                        property.InfoDownload = new PropertyMedia(media);
                        break;
                }

                await _db.SaveChangesAsync();

                string cacheKey = typeof(Content).ToString() + ".Single." + id;
                _cache.Remove(cacheKey);

                return new Response(true, media, $"The media has been attached successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BasePropertyController>($"Error attaching a media file to an entity.", ex);
            }
        }

        /// <summary>
        /// Remove media file from entity.
        /// </summary>
        [HttpPost]
        [Route("admin/property/{id}/media/remove")]
        public virtual async Task<Response> RemoveMedia(int id, AttachMediaModel model)
        {
            try
            {
                // load the media object.
                PropertyListing property = await _db.Properties.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (property == null)
                {
                    throw new Exception("Could not load property to remove media.");
                }

                MediaObject media = _db.Media.SingleOrDefault(m => m.Id == model.MediaId);

                switch (model.FieldName)
                {
                    case nameof(Models.PropertyListing.FeaturedImage):
                        property.FeaturedImageJson = null;
                        break;
                    case nameof(Models.PropertyListing.InfoDownload):
                        property.InfoDownload = null;
                        break;
                }

                await _db.SaveChangesAsync();

                string cacheKey = typeof(PropertyListing).ToString() + ".Single." + id;
                _cache.Remove(cacheKey);

                return new Response(true, MediaObject.Blank, $"The media file has been removed successfully.");

            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BasePropertyController>($"Error removing a media file from an entity.", ex);
            }
        }
        #endregion

        #region Gallery
        [Route("admin/property/{id}/gallery/")]
        public virtual async Task<IActionResult> Gallery(int id)
        {
            PropertyListing model = await _property.GetPropertyByIdAsync(id, true);
            return View("_List_PropertyMedia", model);
        }

        [HttpPost]
        [Route("admin/property/{id}/gallery/upload/")]
        public virtual async Task<Response> UploadToGallery(List<int> media, int id)
        {
            try
            {
                PropertyListing property = await _db.Properties
                        .Include(p => p.Media)
                        .FirstOrDefaultAsync(c => c.Id == id);

                if (property == null)
                {
                    throw new Exception("Property not found!");
                }

                if (media != null)
                {
                    if (media.Count == 0)
                    {
                        throw new Exception("There are no files selected!");
                    }

                    var directory = await _property.GetDirectoryAsync();
                    foreach (int mediaId in media)
                    {
                        // load the media object from db
                        MediaObject mediaObject = _db.Media.AsNoTracking().SingleOrDefault(m => m.Id == mediaId);
                        if (media == null)
                        {
                            throw new Exception("Could not load media to attach.");
                        }
                        var propertyMedia = new PropertyMedia(mediaObject);
                        propertyMedia.PropertyId = property.Id;
                        propertyMedia.Id = 0;
                        _db.PropertyMedia.Add(propertyMedia);
                        await _db.SaveChangesAsync();
                        
                    }
                }
                return new Response(true, "The media has been attached successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BasePropertyController>($"Error attaching media to the gallery.", ex);
            }
        }

        [HttpPost]
        [Route("admin/property/{id}/media/remove/{mediaId}")]
        public virtual async Task<Response> RemoveMedia(int id, int mediaId)
        {
            try
            {
                PropertyMedia media = await _db.PropertyMedia.SingleOrDefaultAsync(m => m.Id == mediaId);
                _db.Entry(media).State = EntityState.Deleted;
                await _db.SaveChangesAsync();
                return new Response(true, $"The media has been removed.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BasePropertyController>($"Error removing media from property.", ex);
            }
        }

        #endregion

        #region Floorplans
        [Route("admin/property/{id}/floorplans/")]
        public virtual async Task<IActionResult> FloorPlans(int id)
        {
            PropertyListing model = await _property.GetPropertyByIdAsync(id, true);
            return View("_List_PropertyFloorplans", model);
        }

        [Route("admin/property/{id}/floorplans/upload")]
        public virtual async Task<Response> UploadFloorplan(List<int> media, int id)
        {
            try
            {
                PropertyListing property = await _db.Properties
                        .Include(p => p.FloorPlans)
                        .FirstOrDefaultAsync(c => c.Id == id);

                if (property == null)
                {
                    throw new Exception("Property not found!");
                }

                if (media != null)
                {
                    if (media.Count == 0)
                    {
                        throw new Exception("There are no files selected!");
                    }

                    var directory = await _property.GetDirectoryAsync();
                    foreach (int mediaId in media)
                    {
                        // load the media object from db
                        MediaObject mediaObject = _db.Media.AsNoTracking().SingleOrDefault(m => m.Id == mediaId);
                        if (media == null)
                        {
                            throw new Exception("Could not load media to attach.");
                        }
                        var propertyMedia = new PropertyFloorplan(mediaObject);
                        propertyMedia.PropertyId = property.Id;
                        propertyMedia.Id = 0;
                        _db.PropertyFloorplans.Add(propertyMedia);
                        await _db.SaveChangesAsync();

                    }
                }
                return new Response(true, "The media has been attached successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BasePropertyController>($"Error uploading a floorplan.", ex);
            }
        }
        [HttpPost]
        [Route("admin/property/{id}/floorplans/remove/{mediaId}")]
        public virtual async Task<Response> RemoveFloorplan(int id, int mediaId)
        {
            try
            {
                PropertyFloorplan media = await _db.PropertyFloorplans.SingleOrDefaultAsync(m => m.Id == mediaId);
                _db.Entry(media).State = EntityState.Deleted;
                await _db.SaveChangesAsync();
                return new Response(true, $"The floorplan has been removed.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BasePropertyController>($"Error removing floorplan from property.", ex);
            }
        }
        #endregion

        #region Features
        [Route("admin/property/{id}/add-meta/")]
        public virtual async Task<IActionResult> AddMeta(int id, string name)
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
                await _logService.AddExceptionAsync<BasePropertyController>(SaveMessage, ex);
            }

            return RedirectToAction(nameof(Edit), new { id });
        }
        [Route("admin/property/{id}/delete-meta/")]
        public virtual async Task<IActionResult> DeleteMeta(int id, int metaId)
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
                await _logService.AddExceptionAsync<BasePropertyController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Edit), new { id });
        }
        #endregion

    }
}


