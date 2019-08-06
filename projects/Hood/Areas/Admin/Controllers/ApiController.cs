using Hood.Controllers;
using Hood.Infrastructure;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin")]
    public class ApiController : BaseController
    {
        public ApiController()
            : base()
        {
        }

        [Route("admin/api/keys/manage/")]
        public async Task<IActionResult> Index(ApiKeyModel model)
        {
            IQueryable<ApiKey> apiKeys = _db.ApiKeys
                .Include(f => f.User)
                .Include(f => f.Events);

            if (!string.IsNullOrEmpty(model.Search))
            {
                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                apiKeys = apiKeys.Where(n => searchTerms.Any(s => n.Name != null && n.Name.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0));
            }

            if (!string.IsNullOrEmpty(model.Order))
            {
                switch (model.Order)
                {
                    case "name":
                    case "title":
                        apiKeys = apiKeys.OrderBy(n => n.Name);
                        break;
                    case "date":
                        apiKeys = apiKeys.OrderBy(n => n.CreatedOn);
                        break;

                    case "name+desc":
                    case "title+desc":
                        apiKeys = apiKeys.OrderByDescending(n => n.Name);
                        break;
                    case "date+desc":
                        apiKeys = apiKeys.OrderByDescending(n => n.CreatedOn);
                        break;

                    default:
                        apiKeys = apiKeys.OrderByDescending(n => n.CreatedOn).ThenBy(n => n.Id);
                        break;
                }
            }

            await model.ReloadAsync(apiKeys);
            return View(model);
        }

        [Route("admin/api/keys/create/")]
        public IActionResult Create()
        {
            ApiKey model = new ApiKey();
            return View(model);
        }

        [HttpPost]
        [Route("admin/api/keys/create/")]
        public async Task<Response> Create(ApiKey model)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByNameAsync(User.Identity.Name);

                // Generate temp slug
                var generator = new KeyGenerator(true, true, true, false);
                model.Key = generator.Generate(24);
                model.UserId = user.Id;
                model.CreatedOn = DateTime.Now;
                model.Active = true;

                _db.ApiKeys.Add(model);
                await _db.SaveChangesAsync();
    #warning TODO: Handle response in JS.
                return new Response(true, "Created successfully.");
            }
            catch (Exception ex)
            {
                var message = $"Error creating an API key";
                await _logService.AddExceptionAsync<ApiController>(message, ex);
                return new Response(ex, message);
            }
        }

        [Route("admin/api/keys/activate/{id}/")]
        [HttpPost]
        public async Task<Response> Activate(string id)
        {
            try
            {
                var model = await _db.ApiKeys
                    .SingleOrDefaultAsync(a => a.Id == id);

                model.Active = true;

                _db.ApiKeys.Update(model);
                await _db.SaveChangesAsync();
#warning TODO: Handle response in JS.
                return new Response(true, "Activated successfully.");
            }
            catch (Exception ex)
            {
                await _logService.AddExceptionAsync<ApiController>($"Error activating an API key", ex);
                return new Response(ex);
            }
        }

        [Route("admin/api/keys/deactivate/{id}")]
        [HttpPost]
        public async Task<Response> Deactivate(string id)
        {
            try
            {
                var model = await _db.ApiKeys
                    .SingleOrDefaultAsync(a => a.Id == id);

                model.Active = false;

                _db.ApiKeys.Update(model);
                await _db.SaveChangesAsync();
#warning TODO: Handle response in JS.
                return new Response(true, "Deactivated successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ApiController>($"Error deactivating an API key.", ex);
            }
        }

        [HttpPost]
        [Route("admin/api/keys/delete/{id}")]
        public async Task<Response> Delete(string id)
        {
            try
            {
                var model = await _db.ApiKeys
                    .Include(a => a.User)
                    .Include(a => a.Events)
                    .SingleOrDefaultAsync(a => a.Id == id); 

                _db.Entry(model).State = EntityState.Deleted;
                _db.SaveChanges();
#warning TODO: Handle response in JS.
                return new Response(true, "Deleted!");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ApiController>($"Error deleting an API key.", ex);
            }
        }

    }
}


