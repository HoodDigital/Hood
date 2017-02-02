using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Hood.Models;
using System.Collections.Generic;
using System.Linq;
using Hood.Services;
using Hood.Extensions;
using Hood.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using System;
using Newtonsoft.Json;
using Hood.Models.Api;
using Microsoft.AspNetCore.Http;
using Hood.Interfaces;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor,Manager")]
    public class PropertyController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPropertyRepository _property;
        private readonly ISiteConfiguration _site;
        private readonly PropertySettings _settings;
        private readonly IHostingEnvironment _env;
        private readonly IBillingService _billing;
        private readonly IMediaManager<SiteMedia> _media;
        private readonly IAddressService _address;

        public PropertyController(
            IPropertyRepository property,
            UserManager<ApplicationUser> userManager,
            ISiteConfiguration site,
            IMediaManager<SiteMedia> media,
            IBillingService billing,
            IHostingEnvironment env,
            IAddressService address)
        {
            _userManager = userManager;
            _property = property;
            _site = site;
            _billing = billing;
            _env = env;
            _media = media;
            _settings = _site.GetPropertySettings();
            _address = address;
        }

        [Route("admin/property/manage/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("admin/property/gallery/{id}/")]
        public IActionResult EditorGallery(int id)
        {
            var model = new PropertyListingApi(_property.GetPropertyById(id, true), _settings);
            return View(model);
        }

        [Route("admin/property/floorplans/{id}/")]
        public IActionResult EditorFloorplans(int id)
        {
            var model = new PropertyListingApi(_property.GetPropertyById(id, true), _settings);
            return View(model);
        }


        [Route("admin/property/edit/{id}/")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await GetEditModel(_property.GetPropertyById(id, true));
            return View(model);
        }

        [HttpPost()]
        [Route("admin/property/edit/{id}/")]
        public async Task<ActionResult> Edit(EditPropertyModelSend post)
        {
            var model = await Save(post);
            return View(model);
        }

        [HttpPost()]
        [Route("admin/property/save/{id}/")]
        public async Task<OperationResult> SaveBlade(EditPropertyModelSend post)
        {
            var model = await Save(post);
            return model.SaveResult;
        }

        private async Task<EditPropertyModel> Save(EditPropertyModelSend post)
        {
            var property = _property.GetPropertyById(post.Id, true);
            post.PublishDate = post.PublishDate.AddHours(post.PublishHour);
            post.PublishDate = post.PublishDate.AddMinutes(post.PublishMinute);
            // Try to map the address (Only works with UK)
            if (post.Postcode != property.Postcode)
                post.SetLocation(_address.GeocodeAddress(post));
            post.CopyProperties(property);
            property.AgentId = post.AgentId;
            var type = _site.GetPropertySettings().GetPlanningFromType(post.Planning);
            if (property.HasMeta("PlanningDescription"))
                property.UpdateMeta("PlanningDescription", type);
            else
            {
                if (property.Metadata == null)
                    property.Metadata = new List<PropertyMeta>();
                property.AddMeta(new PropertyMeta("PlanningDescription", type));
            }
            property.LastEditedBy = User.Identity.Name;
            property.LastEditedOn = DateTime.Now;
            OperationResult result = _property.UpdateProperty(property);
            var model = await GetEditModel(property);
            model.SaveResult = result;
            return model;
        }
        private async Task<EditPropertyModel> GetEditModel(PropertyListing listing)
        {
            EditPropertyModel model = new EditPropertyModel()
            {
                Property = listing
            };
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var editors = await _userManager.GetUsersInRoleAsync("Editor");
            model.Agents = editors.Concat(admins).Distinct().OrderBy(u => u.FirstName).ThenBy(u => u.Email).ToList();
            return model;
        }


        [Route("admin/property/create/")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "SuperUser,Admin")]
        [Route("admin/property/delete/all/")]
        public async Task<IActionResult> DeleteAll()
        {
            await _property.DeleteAll();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("admin/property/get")]
        public async Task<JsonResult> Get(ListFilters filters, string type, string planning, bool all = false, bool published = false)
        {
            string agent = User.Identity.Name;
            if (all && User.IsInRole("Admin"))
                agent = null;

            PropertyFilters propertyFilters = new PropertyFilters();
            filters.CopyProperties(propertyFilters);
            propertyFilters.Agent = agent;
            propertyFilters.Type = type;
            propertyFilters.PlanningType = planning;

            PagedList<PropertyListing> properties = await _property.GetPagedProperties(propertyFilters, published);
            Response response = new Response(properties.Items.Select(p => _site.ToPropertyListingApi(p)).ToArray(), properties.Count);
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            return Json(response, settings);
        }

        [HttpPost]
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

                // Geocode
                property.SetLocation(_address.GeocodeAddress(property));

                OperationResult result = _property.Add(property);
                if (property.Metadata == null)
                    property.Metadata = new List<PropertyMeta>();
                property.UpdateMeta("PlanningDescription", _site.GetPropertySettings().GetPlanningTypes().FirstOrDefault().Value);
                result = _property.UpdateProperty(property);
                if (!result.Succeeded)
                {
                    throw new Exception(result.ErrorString);
                }

                return new Response(true);

            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/property/delete")]
        [HttpPost()]
        public Response Delete(int id)
        {
            try
            {
                OperationResult result = _property.Delete(id);
                if (result.Succeeded)
                {
                    return new Response(true);
                }
                else
                {
                    return new Response("There was a problem updating the database");
                }
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/property/publish")]
        [HttpPost()]
        public Response Publish(int id)
        {
            try
            {
                OperationResult<PropertyListing> result = _property.SetStatus(id, Status.Published);
                if (result.Succeeded)
                {
                    return new Response(true);
                }
                else
                {
                    return new Response("There was a problem updating the database");
                }
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/property/archive")]
        [HttpPost()]
        public Response Archive(int id)
        {
            try
            {
                OperationResult<PropertyListing> result = _property.SetStatus(id, Status.Archived);
                if (result.Succeeded)
                {
                    return new Response(true);
                }
                else
                {
                    return new Response("There was a problem updating the database");
                }
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
        public async Task<IActionResult> UploadToGallery(List<IFormFile> files, int id)
        {
            // User must have an organisation.
            PropertyListing property = _property.GetPropertyById(id);
            if (property == null)
                return NotFound();
            AccountInfo account = HttpContext.GetAccountInfo();

            try
            {
                SiteMedia mediaResult = null;
                if (files != null)
                {
                    if (files.Count == 0)
                        return Json(new
                        {
                            Success = false,
                            Error = "There are no files attached!"
                        });

                    foreach (IFormFile file in files)
                    {
                        mediaResult = await _media.ProcessUpload(file, new SiteMedia() { Directory = "Property" });
                        await _property.AddImage(property, new PropertyMedia(mediaResult));
                    }
                }
                return Json(new { Success = true, Image = mediaResult });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
            }
        }

        /// <summary>
        /// This adds images to the club gallery.
        /// </summary>
        /// <param name="clubSlug">The SEO slug for the club</param>
        /// <returns></returns>
        [Authorize]
        [Route("admin/property/upload/floorplan")]
        public async Task<IActionResult> UploadFloorplan(List<IFormFile> files, int id)
        {
            // User must have an organisation.
            PropertyListing property = _property.GetPropertyById(id);
            if (property == null)
                return NotFound();
            AccountInfo account = HttpContext.GetAccountInfo();

            try
            {
                SiteMedia mediaResult = null;
                if (files != null)
                {
                    if (files.Count == 0)
                        return Json(new
                        {
                            Success = false,
                            Error = "There are no files attached!"
                        });

                    foreach (IFormFile file in files)
                    {
                        mediaResult = await _media.ProcessUpload(file, new SiteMedia() { Directory = "Property" });
                        await _property.AddFloorplan(property, new PropertyFloorplan(mediaResult));
                    }
                }
                return Json(new { Success = true, Image = mediaResult });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
            }
        }

        [HttpGet]
        public MediaApi GetMedia(int id, string type)
        {
            try
            {
                PropertyListing property = _property.GetPropertyById(id, true);
                if (property != null)
                    switch (type)
                    {
                        case "FeaturedImage":
                            return new MediaApi(property.FeaturedImage);
                        case "InfoDownload":
                            return new MediaApi(property.InfoDownload);
                        default:
                            return MediaApi.Blank(_site.GetMediaSettings());
                    }
                else
                    throw new Exception("No media found.");
            }
            catch (Exception)
            {
                return MediaApi.Blank(_site.GetMediaSettings());
            }
        }

        [HttpGet]
        public Response ClearField(int id, string field)
        {
            try
            {
                _property.ClearField(id, field);
                return new Response(true, "The field has been cleared!");
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [HttpGet]
        [Route("admin/property/media/setfeatured/{id}/{mediaId}")]
        public IActionResult SetFeatured(int id, int mediaId)
        {
            try
            {
                PropertyListing property = _property.GetPropertyById(id, true);
                PropertyMedia media = property.Media.SingleOrDefault(m => m.Id == mediaId);
                if (media != null)
                {
                    property.FeaturedImage = new SiteMedia(media);
                    _property.UpdateProperty(property);
                }
            }
            catch (Exception)
            { }
            return RedirectToAction("Edit", new { id = id });
        }

        [HttpGet]
        [Route("admin/property/media/remove/{id}/{mediaId}")]
        public IActionResult RemoveMedia(int id, int mediaId)
        {
            try
            {
                PropertyListing property = _property.GetPropertyById(id, true);
                PropertyMedia media = property.Media.Find(m => m.Id == mediaId);
                if (media != null)
                    property.Media.Remove(media);
                _property.UpdateProperty(property);
            }
            catch (Exception)
            { }
            return RedirectToAction("Edit", new { id = id });
        }

        [HttpGet]
        [Route("admin/property/floorplan/remove/{id}/{mediaId}")]
        public IActionResult RemoveFloorplan(int id, int mediaId)
        {
            try
            {
                PropertyListing property = _property.GetPropertyById(id, true);
                PropertyFloorplan media = property.FloorPlans.Find(m => m.Id == mediaId);
                if (media != null)
                    property.FloorPlans.Remove(media);
                _property.UpdateProperty(property);
            }
            catch (Exception)
            { }
            return RedirectToAction("Edit", new { id = id });
        }


    }
}


