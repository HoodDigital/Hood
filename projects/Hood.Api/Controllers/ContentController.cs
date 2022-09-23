using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Hood.Models;
using Hood.ViewModels;
using Hood.Services;
using Hood.Contexts;
using Hood.Caching;
using Hood.Core;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hood.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContentController : Hood.Api.BaseControllers.ContentController
    {
        public ContentController() : base()
        { }
    }
}
