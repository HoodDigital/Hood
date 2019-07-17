using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Index(PropertyListModel model) => await List(model, "Index");

        [Route("admin/property/list/")]
        public async Task<IActionResult> List(PropertyListModel model, string viewName)
        {
            var propertySettings = Engine.Settings.Property;
            if (!propertySettings.Enabled || !propertySettings.ShowList)
                return NotFound();

            model = await _property.GetPropertiesAsync(model);

            model.Locations = await _property.GetLocationsAsync(model);
            model.CentrePoint = GeoCalculations.GetCentralGeoCoordinate(model.Locations.Select(p => new GeoCoordinate(p.Latitude, p.Longitude)));

            PropertySettings settings = Engine.Settings.Property;

            model.Types = settings.GetListingTypes();
            model.PlanningTypes = settings.GetPlanningTypes();

            return View(viewName.IsSet() ? viewName : "_List_Property", model);
        }

        #region Edit
        [Route("admin/property/edit/{id}/")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _property.GetPropertyByIdAsync(id, true);
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
                model.PublishDate = model.PublishDate.AddHours(model.PublishHour);
                model.PublishDate = model.PublishDate.AddMinutes(model.PublishMinute);
                model.LastEditedBy = User.Identity.Name;
                model.LastEditedOn = DateTime.Now;

                if (model.AutoGeocode)
                {
                    var address = _address.GeocodeAddress(model);
                    if (address != null)
                    {
                        model.SetLocation(address.Coordinates);
                    }
                }

                await _property.UpdateAsync(model);

                var type = Engine.Settings.Property.GetPlanningFromType(model.Planning);
                if (model.HasMeta("PlanningDescription"))
                    model.UpdateMeta("PlanningDescription", type);
                else
                {
                    if (model.Metadata == null)
                        model.Metadata = new List<PropertyMeta>();
                    model.AddMeta(new PropertyMeta("PlanningDescription", type));
                }

                foreach (var val in Request.Form)
                {
                    if (val.Key.StartsWith("Meta:"))
                    {
                        if (model.HasMeta(val.Key.Replace("Meta:", "")))
                        {
                            model.UpdateMeta(val.Key.Replace("Meta:", ""), val.Value.ToString());
                        }
                        else
                        {
                            model.AddMeta(new PropertyMeta()
                            {
                                PropertyId = property.Id,
                                Name = val.Key.Replace("Meta:", ""),
                                Type = "System.String",
                                BaseValue = JsonConvert.SerializeObject(val.Value)
                            });
                        }
                    }
                }

                await _property.UpdateAsync(property);
                model = await LoadAgents(model);
                model.SaveMessage = "Saved";
                model.MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred: " + ex.Message;
                model.MessageType = AlertType.Danger;
            }
            return View(model);
        }
        private async Task<PropertyListing> LoadAgents(PropertyListing listing)
        {
#warning Replace this with client side user lookup.
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var editors = await _userManager.GetUsersInRoleAsync("Editor");
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

                var leaseStatuses = _propertySettings.GetLeaseStatuses();
                if (leaseStatuses.Count > 0)
                    model.LeaseStatus = leaseStatuses.FirstOrDefault();
                else
                    model.LeaseStatus = "Available";

                var planningTypes = _propertySettings.GetPlanningTypes();
                if (planningTypes.Count > 0)
                    model.Planning = planningTypes.FirstOrDefault().Key;
                else
                    model.Planning = "VAR";

                var listingTypes = _propertySettings.GetListingTypes();
                if (listingTypes.Count > 0)
                    model.ListingType = listingTypes.FirstOrDefault();
                else
                    model.ListingType = "Not Specified";

                // Geocode
                var address = _address.GeocodeAddress(model);
                if (address != null)
                {
                    model.SetLocation(address.Coordinates);
                }

                await _property.AddAsync(model);
                if (model.Metadata == null)
                    model.Metadata = new List<PropertyMeta>();
                model.UpdateMeta("PlanningDescription", Engine.Settings.Property.GetPlanningTypes().FirstOrDefault().Value);

                for (int i = 0; i < 11; i++)
                {
                    model.AddMeta(new PropertyMeta()
                    {
                        PropertyId = model.Id,
                        Name = "Feature" + i.ToString(),
                        Type = "System.String",
                        BaseValue = JsonConvert.SerializeObject("")
                    });
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
        [Route("admin/property/delete/all/")]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                await _property.DeleteAllAsync();
                SaveMessage = $"All the properties have been deleted.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error deleting all properties.";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<PropertyController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Index));
        }

        [Route("admin/property/delete/{id}")]
        [HttpPost()]
        public async Task<Response> Delete(int id)
        {
            try
            {
                await _property.DeleteAsync(id);
                var response = new Response(true, "The property has been successfully deleted.");
                return response;
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
            var model = await _property.GetPropertyByIdAsync(id, true);
            return View(model);
        }

        [Authorize]
        [Route("admin/property/upload/gallery")]
        public async Task<Response> UploadToGallery(List<IFormFile> files, int id)
        {
            try
            {
                PropertyListing property = await _property.GetPropertyByIdAsync(id);
                if (property == null)
                    throw new Exception("Property not found!");

                MediaObject mediaResult = null;
                if (files != null)
                {
                    if (files.Count == 0)
                        throw new Exception("There are no files attached!");

                    foreach (IFormFile file in files)
                    {
                        mediaResult = await _media.ProcessUpload(file, property.DirectoryPath) as MediaObject;
                        await _property.AddMediaAsync(property, new PropertyMedia(mediaResult));
                    }
                }
                return new Response(true, mediaResult);
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error uploading media to the gallery.", ex);
            }
        }

        [HttpGet]
        [Route("admin/property/media/setfeatured/{id}/{mediaId}")]
        public async Task<IActionResult> SetFeatured(int id, int mediaId)
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
                SaveMessage = $"Featured image has been updated.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error setting featured image";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<PropertyController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpGet]
        [Route("admin/property/media/remove/{id}/{mediaId}")]
        public async Task<Response> RemoveMedia(int id, int mediaId)
        {
            try
            {
                PropertyListing property = await _property.RemoveMediaAsync(id, mediaId);
                return new Response(true, $"The media has been removed.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error removing media.", ex);
            }
        }

        #endregion

        #region Floorplans
        [Route("admin/property/floorplans/{id}/")]
        public async Task<IActionResult> FloorPlans(int id)
        {
            var model = await _property.GetPropertyByIdAsync(id, true);
            return View(model);
        }

        [Authorize]
        [Route("admin/property/upload/floorplan")]
        public async Task<Response> UploadFloorplan(List<IFormFile> files, int id)
        {
            try
            {
                PropertyListing property = await _property.GetPropertyByIdAsync(id);
                if (property == null)
                    throw new Exception("Property not found!");

                MediaObject mediaResult = null;
                if (files != null)
                {
                    if (files.Count == 0)
                        throw new Exception("There are no files attached!");

                    foreach (IFormFile file in files)
                    {
                        mediaResult = await _media.ProcessUpload(file, property.DirectoryPath) as MediaObject;
                        await _property.AddFloorplanAsync(property, new PropertyFloorplan(mediaResult));
                    }
                }
                return new Response(true, mediaResult);
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error uploading a floorplan.", ex);
            }
        }
        [HttpGet]
        [Route("admin/property/floorplan/remove/{id}/{mediaId}")]
        public async Task<Response> RemoveFloorplan(int id, int mediaId)
        {
            try
            {
                PropertyListing property = await _property.RemoveFloorplanAsync(id, mediaId);
                return new Response(true, $"The floorplan has been removed.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<PropertyController>($"Error removing floorplan.", ex);
            }
        }
        #endregion

    }
}


