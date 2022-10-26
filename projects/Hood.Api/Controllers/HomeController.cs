using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Hood.Models;

namespace Hood.Api.Controllers
{
    [ApiController]
    public class HomeController : Hood.Api.BaseControllers.HomeController
    {
        public HomeController() : base()
        { }
    }
}
