using Hood.Controllers;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Hood.Identity.Policies.Active, Roles = "SuperUser,Admin,Editor")]

    public class HomeController : BaseHomeController
    {
        public HomeController()
            : base()
        {
        }
    }
}
