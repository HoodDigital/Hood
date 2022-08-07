using Hood.Admin.BaseControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Hood.Identity.Policies.Active, Roles = "SuperUser,Admin,Editor")]

    public class ContentController : BaseContentController
    {
        public ContentController()
            : base()
        {
        }
    }

}


