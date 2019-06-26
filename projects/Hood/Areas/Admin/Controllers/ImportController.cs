using Hood.Core;
using Hood.Controllers;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class ImportController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        private readonly IFTPService _ftp;
        private readonly IPropertyImporter _blm;

        //private readonly IContentExporter _contentExporter;
        //private readonly IPropertyExporter _propertyExporter;

        public ImportController(
            IFTPService ftp, 
            IPropertyImporter blm
            //IContentExporter contentExporter, 
            //IPropertyExporter propertyExporter
            )
            : base()
        {
            _ftp = ftp;
            _blm = blm;
            //_contentExporter = contentExporter;
            //_propertyExporter = propertyExporter;
        }

        #region "BlmPropertyImporter"

        [HttpPost]
        [Route("admin/property/import/blm/trigger")]
        [AllowAnonymous]
        public IActionResult BlmPropertyImporterTrigger()
        {
            var triggerAuth = Engine.Settings.Property.TriggerAuthKey;
            if (Request.Headers.ContainsKey("Auth") && Request.Headers["Auth"] == triggerAuth && !_blm.IsRunning())
            {
                _blm.RunUpdate(HttpContext);
                return StatusCode(200);
            }

            StringWriter logWriter = new StringWriter();
            logWriter.WriteLine("Unauthorized attempt from " + HttpContext.Connection.RemoteIpAddress.ToString());
            logWriter.WriteLine("Auth Key: " + triggerAuth);
            logWriter.WriteLine("Auth Header: " + Request.Headers["Auth"]);
            logWriter.WriteLine("Blm Importer Status: " + (_blm.IsRunning() ? "True" : "False"));
            var report = _blm.Report();
            var status = JsonConvert.SerializeObject(report);
            logWriter.WriteLine("Blm Importer Report: ");
            logWriter.Write(status.ToFormattedJson());

            _env.WriteLogToFile<IPropertyImporter>(logWriter.ToString());

            return StatusCode(401);
        }

        [Route("admin/property/import/blm/")]
        public IActionResult BlmImporter()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/property/import/blm/start/")]
        public IActionResult BlmImporterStart()
        {
            _blm.Kill();
            _blm.RunUpdate(HttpContext);
            return Json(new { success = true });
        }

        [HttpPost]
        [Route("admin/property/import/blm/cancel/")]
        public IActionResult BlmImporterCancel()
        {
            _blm.Kill();
            return Json(new { success = true });
        }

        [Route("admin/property/import/blm/status/")]
        public IActionResult BlmImporterStatus()
        {
            return Json(new
            {
                Importer = _blm.Report(),
                Ftp = _ftp.Report()
            });
        }

        #endregion

        //#region "Content"

        //[Route("admin/content/export/")]
        //public IActionResult ContentExport()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[Route("admin/content/export/start/")]
        //public IActionResult ContentExportStart()
        //{
        //    _contentExporter.Kill();
        //    _contentExporter.ExportContent(HttpContext);
        //    return Json(new { success = true });
        //}

        //[HttpPost]
        //[Route("admin/content/export/cancel/")]
        //public IActionResult ContentExportCancel()
        //{
        //    _contentExporter.Kill();
        //    return Json(new { success = true });
        //}

        //[Route("admin/content/export/status/")]
        //public IActionResult ContentExportStatus()
        //{
        //    return Json(_contentExporter.Report());
        //}

        //#endregion

        //#region "Properties"

        //[Route("admin/properties/export/")]
        //public IActionResult PropertiesExport()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[Route("admin/properties/export/start/")]
        //public IActionResult PropertiesExportStart()
        //{
        //    _propertyExporter.Kill();
        //    _propertyExporter.ExportProperties(HttpContext);
        //    return Json(new { success = true });
        //}

        //[HttpPost]
        //[Route("admin/properties/export/cancel/")]
        //public IActionResult PropertiesExportCancel()
        //{
        //    _propertyExporter.Kill();
        //    return Json(new { success = true });
        //}

        //[Route("admin/properties/export/status/")]
        //public IActionResult PropertiesExportStatus()
        //{
        //    return Json(_propertyExporter.Report());
        //}

        //#endregion


    }
}
