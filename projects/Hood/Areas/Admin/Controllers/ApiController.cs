using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Interfaces;
using Hood.IO;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor,Manager")]
    public class ApiController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        public ApiController()
            : base()
        {
        }

        [Route("admin/api/keys/manage/")]
        public async Task<IActionResult> Index(ApiKeyModel model, EditorMessage? message)
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
            model.AddEditorMessage(message);
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

                var response = new Response(true, "Created successfully.");
                response.Url = Url.Action("Index", new { message = EditorMessage.Created });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/api/keys/activate/{id}/")]
        [HttpPost()]
        public async Task<Response> Activate(string id)
        {
            try
            {
                var model = await _db.ApiKeys
                    .SingleOrDefaultAsync(a => a.Id == id);

                model.Active = true;

                _db.ApiKeys.Update(model);
                await _db.SaveChangesAsync();

                var response = new Response(true, "Activated successfully.");
                response.Url = Url.Action("Index", new { id = model.Id, message = EditorMessage.Activated });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/api/keys/deactivate/{id}")]
        [HttpPost()]
        public async Task<Response> Deactivate(string id)
        {
            try
            {
                var model = await _db.ApiKeys
                    .SingleOrDefaultAsync(a => a.Id == id);

                model.Active = false;

                _db.ApiKeys.Update(model);
                await _db.SaveChangesAsync();

                var response = new Response(true, "Deactivated successfully.");
                response.Url = Url.Action("Index", new { id = model.Id, message = EditorMessage.Deactivated });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [HttpPost()]
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

                var response = new Response(true, "Deleted!");
                response.Url = Url.Action("Index", new { message = EditorMessage.Deleted });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

    }
}


