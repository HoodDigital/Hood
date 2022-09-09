using Hood.Admin.BaseControllers;
using Hood.Constants.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin,Editor")]

    public class HomeController : BaseHomeController
    {
        public HomeController()
            : base()
        {
        }
    }
}
