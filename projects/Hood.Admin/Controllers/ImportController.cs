using Hood.Core;
using Hood.Admin.BaseControllers;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Hood.Constants.Identity;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin")]

    public class ImportController : BaseImportController
    {
        public ImportController()
            : base()
        {
        }
    }
}
