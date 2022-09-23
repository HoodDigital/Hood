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

namespace Hood.Api.BaseControllers
{
    [ApiController]
    [Route("[controller]")]
    public abstract class ContentController : Controller
    {
        protected readonly ContentContext _contentDb;
        protected readonly IContentRepository _content;
        protected readonly ContentCategoryCache _contentCategoryCache;

        public ContentController()
            : base()
        {
            _contentDb = Engine.Services.Resolve<ContentContext>();
            _content = Engine.Services.Resolve<IContentRepository>();
            _contentCategoryCache = Engine.Services.Resolve<ContentCategoryCache>();
        }

        [HttpGet("")]
        public async Task<IActionResult> IndexAsync(string type, int pageIndex = 1, int pageSize = 20)
        {
            var model = await _content.GetContentAsync(new ContentModel()
            {
                Type = type,
                PageIndex = pageIndex,
                PageSize = pageSize
            });
            return Json(new
            {
                data = model.List,
                pagination = new
                {
                    pageIndex,
                    pageSize,
                    count = model.TotalCount, 
                    totalPages = model.TotalPages
                }
            });
        }

    }
}
