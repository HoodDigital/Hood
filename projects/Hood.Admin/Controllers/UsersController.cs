using Hood.Admin.BaseControllers;
using Hood.Constants.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin")]
    // Standard ASP.NET Identity is the default backend; this admin controller derives from the
    // standard base. Auth0 deployments substitute Hood.Admin.BaseControllers.Auth0UsersController.
    public class UsersController : Hood.Admin.BaseControllers.UsersController
    {
        public UsersController()
            : base() { }
    }
}
