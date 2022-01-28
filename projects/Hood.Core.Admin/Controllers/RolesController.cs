using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Identity;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    public abstract class BaseRolesController : BaseController
    {
        public BaseRolesController()
            : base()
        { }

        [Route("admin/roles/")]
        public virtual async Task<IActionResult> Index(IPagedList<IdentityRole> model)
        {
            return await List(model, "Index");
        }

        [HttpGet]
        [Route("admin/roles/list/")]
        public virtual async Task<IActionResult> List(IPagedList<IdentityRole> model, string viewName = "_List_Roles")
        {
            model = await _account.GetRolesAsync(model);
            return View(viewName, model);
        }

        #region Create
        [Route("admin/roles/create/")]
        public virtual IActionResult Create()
        {
            return View();
        }

        [Route("admin/roles/create/")]
        [HttpPost]
        public virtual async Task<Response> Create(AdminCreateRoleViewModel model)
        {
            try
            {
                await _account.CreateRoleAsync(model.Name);
                return new Response(true, "Successfully created.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BaseRolesController>($"Error creating a role via the admin panel.", ex);
            }
        }
        #endregion

        #region Delete
        [Route("admin/roles/{id}/delete/")]
        [HttpPost]
        public virtual async Task<Response> Delete(string id)
        {
            try
            {
                IdentityRole role = await _account.GetRoleAsync(id);
                if (role == null)
                {
                    throw new Exception($"The role Id {id} could not be found, therefore could not be deleted.");
                }

                await _account.DeleteRoleAsync(id);
                await _logService.AddLogAsync<BaseRolesController>($"The role ({role.Name}) has been deleted via the admin area by {User.Identity.Name}", type: LogType.Warning);
                return new Response(true, "Deleted successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<BaseRolesController>($"Error deleting a role via the admin panel.", ex);
            }
        }
        #endregion
    }
}
