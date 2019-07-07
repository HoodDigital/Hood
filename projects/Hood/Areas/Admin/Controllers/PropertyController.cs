using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Interfaces;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        public async Task<IActionResult> Index(PropertyListModel model, EditorMessage? message)
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

            model.AddEditorMessage(message);
            return View(model);
        }

        [Route("admin/property/gallery/{id}/")]
        public async Task<IActionResult> EditorGalleryAsync(int id)
        {
            var model = await _property.GetPropertyByIdAsync(id, true);
            return View(model);
        }

        [Route("admin/property/floorplans/{id}/")]
        public async Task<IActionResult> EditorFloorplans(int id)
        {
            var model = await _property.GetPropertyByIdAsync(id, true);
            return View(model);
        }


        [Route("admin/property/edit/{id}/")]
        public async Task<IActionResult> Edit(int id, EditorMessage? message)
        {
            var model = await _property.GetPropertyByIdAsync(id, true);
            model = await LoadAgents(model);
            model.AutoGeocode = true;
            model.AddEditorMessage(message);
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

                var property = await _property.GetPropertyByIdAsync(model.Id, true);
                var type = Engine.Settings.Property.GetPlanningFromType(model.Planning);

                if (property.HasMeta("PlanningDescription"))
                    property.UpdateMeta("PlanningDescription", type);
                else
                {
                    if (property.Metadata == null)
                        property.Metadata = new List<PropertyMeta>();
                    property.AddMeta(new PropertyMeta("PlanningDescription", type));
                }

                foreach (var val in Request.Form)
                {
                    if (val.Key.StartsWith("Meta:"))
                    {
                        if (property.HasMeta(val.Key.Replace("Meta:", "")))
                        {
                            property.UpdateMeta(val.Key.Replace("Meta:", ""), val.Value.ToString());
                        }
                        else
                        {
                            property.AddMeta(new PropertyMeta()
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
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var editors = await _userManager.GetUsersInRoleAsync("Editor");
            listing.AvailableAgents = editors.Concat(admins).Distinct().OrderBy(u => u.FirstName).ThenBy(u => u.Email).ToList();
            return listing;
        }

        [Route("admin/property/create/")]
        public IActionResult Create()
        {
            return View();
        }

        [Route("admin/property/addfeature/{id}/")]
        public async Task<IActionResult> AddFeature(int id)
        {
            var property = await _property.GetPropertyByIdAsync(id, true);
            int? count = property.Metadata?.Where(m => m.Name.Contains("Feature")).Count();
            if (!count.HasValue)
                count = 0;

            property.AddMeta(new PropertyMeta()
            {
                PropertyId = property.Id,
                Name = "Feature" + (count + 1).ToString(),
                Type = "System.String",
                BaseValue = JsonConvert.SerializeObject("")
            });
            await _property.UpdateAsync(property);

            return RedirectToAction("Edit", new { id = property.Id });
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [Route("admin/property/delete/all/")]
        public async Task<IActionResult> DeleteAll()
        {
            await _property.DeleteAllAsync();
            return RedirectToAction("Index", new { message = EditorMessage.Deleted });
        }

        [HttpPost]
        [Route("admin/property/add/")]
        public async Task<Response> Add(CreatePropertyModel model)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByNameAsync(User.Identity.Name);

                PropertyListing property = new PropertyListing
                {
                    AgentId = user.Id,
                    CreatedBy = user.UserName,
                    CreatedOn = DateTime.Now,
                    Number = model.cpNumber,
                    Address1 = model.cpAddress1,
                    Address2 = model.cpAddress2,
                    City = model.cpCity,
                    County = model.cpCounty,
                    Country = model.cpCountry,
                    Postcode = model.cpPostcode,
                    LastEditedBy = user.UserName,
                    LastEditedOn = DateTime.Now,
                    Confidential = false,
                    Featured = false,
                    AskingPrice = 0,
                    Fees = 0,
                    Rent = 0,
                    Premium = 0,
                    AskingPriceDisplay = "{0}",
                    FeesDisplay = "{0}",
                    RentDisplay = "{0}",
                    PremiumDisplay = "{0}",
                    PublishDate = new DateTime(model.cpPublishDate.Year, model.cpPublishDate.Month, model.cpPublishDate.Day, model.cpPublishHour, model.cpPublishMinute, 0),
                    Status = model.cpStatus,
                    Title = model.cpTitle,
                    ShareCount = 0,
                    Views = 0,
                    Latitude = model.cpLatitude,
                    Longitude = model.cpLongitude
                };

                var leaseStatuses = _propertySettings.GetLeaseStatuses();
                if (leaseStatuses.Count > 0)
                    property.LeaseStatus = leaseStatuses.FirstOrDefault();
                else
                    property.LeaseStatus = "Available";

                var planningTypes = _propertySettings.GetPlanningTypes();
                if (planningTypes.Count > 0)
                    property.Planning = planningTypes.FirstOrDefault().Key;
                else
                    property.Planning = "VAR";

                var listingTypes = _propertySettings.GetListingTypes();
                if (listingTypes.Count > 0)
                    property.ListingType = listingTypes.FirstOrDefault();
                else
                    property.ListingType = "Not Specified";

                // Geocode
                var address = _address.GeocodeAddress(property);
                if (address != null)
                {
                    property.SetLocation(address.Coordinates);
                }

                await _property.AddAsync(property);
                if (property.Metadata == null)
                    property.Metadata = new List<PropertyMeta>();
                property.UpdateMeta("PlanningDescription", Engine.Settings.Property.GetPlanningTypes().FirstOrDefault().Value);

                for (int i = 0; i < 11; i++)
                {
                    property.AddMeta(new PropertyMeta()
                    {
                        PropertyId = property.Id,
                        Name = "Feature" + i.ToString(),
                        Type = "System.String",
                        BaseValue = JsonConvert.SerializeObject("")
                    });
                }

                await _property.UpdateAsync(property);

                var response = new Response(true, "Published successfully.")
                {
                    Url = Url.Action("Edit", new { id = property.Id, message = EditorMessage.Created })
                };
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/property/delete")]
        [HttpPost()]
        public async Task<Response> DeleteAsync(int id)
        {
            try
            {
                await _property.DeleteAsync(id);
                var response = new Response(true, "Deleted!");
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/property/publish")]
        [HttpPost()]
        public async Task<Response> Publish(int id)
        {
            try
            {
                await _property.SetStatusAsync(id, ContentStatus.Published);
                return new Response(true, "Published successfully.");
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/property/archive")]
        [HttpPost()]
        public async Task<Response> Archive(int id)
        {
            try
            {
                await _property.SetStatusAsync(id, ContentStatus.Archived);
                return new Response(true, "Archived successfully.");
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        /// <summary>
        /// This adds images to the club gallery.
        /// </summary>
        /// <param name="clubSlug">The SEO slug for the club</param>
        /// <returns></returns>
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
                        mediaResult = await _media.ProcessUpload(file, new MediaObject() { Directory = "Property" });
                        await _property.AddMediaAsync(property, new PropertyMedia(mediaResult));
                    }
                }
                return new Response(true, mediaResult);
            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }

        /// <summary>
        /// This adds images to the club gallery.
        /// </summary>
        /// <param name="clubSlug">The SEO slug for the club</param>
        /// <returns></returns>
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
                        mediaResult = await _media.ProcessUpload(file, new MediaObject() { Directory = "Property" });
                        await _property.AddFloorplanAsync(property, new PropertyFloorplan(mediaResult));
                    }
                }
                return new Response(true, mediaResult);
            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }

        [HttpGet]
        [Route("admin/property/getmedia/")]
        public async Task<IMediaObject> GetMediaAsync(int id, string type)
        {
            try
            {
                PropertyListing property = await _property.GetPropertyByIdAsync(id, true);
                if (property != null)
                    switch (type)
                    {
                        case "FeaturedImage":
                            return property.FeaturedImage;
                        case "InfoDownload":
                            return property.InfoDownload;
                        default:
                            return MediaObject.Blank;
                    }
                else
                    throw new Exception("No media found.");
            }
            catch (Exception)
            {
                return MediaObject.Blank;
            }
        }

        [HttpGet]
        [Route("admin/property/clearfield/")]
        public async Task<Response> ClearFieldAsync(int id, string field)
        {
            try
            {
                await _property.ClearFieldAsync(id, field);
                return new Response(true, "The field has been cleared!");
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
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
            }
            catch (Exception)
            { }
            return RedirectToAction("Edit", new { id, message = EditorMessage.ImageUpdated });
        }

        [HttpGet]
        [Route("admin/property/media/remove/{id}/{mediaId}")]
        public async Task<IActionResult> RemoveMedia(int id, int mediaId)
        {
            try
            {
                PropertyListing property = await _property.RemoveMediaAsync(id, mediaId);
            }
            catch (Exception)
            { }
            return RedirectToAction("Edit", new { id, message = EditorMessage.MediaRemoved });
        }

        [HttpGet]
        [Route("admin/property/floorplan/remove/{id}/{mediaId}")]
        public async Task<IActionResult> RemoveFloorplan(int id, int mediaId)
        {
            try
            {
                PropertyListing property = await _property.RemoveFloorplanAsync(id, mediaId);
            }
            catch (Exception)
            { }
            return RedirectToAction("Edit", new { id, message = EditorMessage.MediaRemoved });
        }


    }
}


