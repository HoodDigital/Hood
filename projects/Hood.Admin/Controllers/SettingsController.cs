using Hood.Admin.BaseControllers;
using Hood.Constants.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin")]
    public class SettingsController : BaseSettingsController
    {
        public SettingsController()
            : base()
        {
        }
    }
}
