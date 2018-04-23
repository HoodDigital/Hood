using Hood.Extensions;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor,Manager")]
    public class ImportController : Controller
    {
        private readonly IRightmovePropertyImporter _rightmove;
        private readonly IPropertyExporter _propertyExporter;
        private readonly IContentExporter _contentExporter;
        private readonly ISettingsRepository _settings;
        private readonly IFTPService _ftp;
        private readonly IHostingEnvironment _env;

        public ImportController(IFTPService ftp, 
            ISettingsRepository settings,
            IRightmovePropertyImporter rightmove,
            IHostingEnvironment env
            //IContentExporter contentExporter, 
            //IPropertyExporter propertyExporter
            )
        {
            _settings = settings;
            _ftp = ftp;
            _env = env;
            _rightmove = rightmove;
            //_contentExporter = contentExporter;
            //_propertyExporter = propertyExporter;
        }

        #region "RightmovePropertyImporter"

        [HttpPost]
        [Route("admin/property/import/rightmove/trigger")]
        [AllowAnonymous]
        public IActionResult RightmovePropertyImporterTrigger()
        {
            var triggerAuth = _settings.GetPropertySettings().TriggerAuthKey;
            if (Request.Headers.ContainsKey("Auth") && Request.Headers["Auth"] == triggerAuth && !_rightmove.IsRunning())
            {
                _rightmove.RunUpdate(HttpContext);
                return StatusCode(200);
            }

            StringWriter logWriter = new StringWriter();
            logWriter.WriteLine("Unauthorized attempt from " + HttpContext.Connection.RemoteIpAddress.ToString());
            logWriter.WriteLine("Auth Header: " + Request.Headers["Auth"]);
            logWriter.WriteLine("Rightmove Importer Status: " + (_rightmove.IsRunning() ? "True" : "False"));
            var report = _rightmove.Report();
            var status = JsonConvert.SerializeObject(report);
            logWriter.WriteLine("Rightmove Importer Report: ");
            logWriter.Write(status.ToFormattedJson());

            _env.WriteLogToFile<IRightmovePropertyImporter>(logWriter.ToString());

            return StatusCode(401);
        }

        [Route("admin/property/import/rightmove/")]
        public IActionResult PropertyFTP()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/property/import/rightmove/start/")]
        public IActionResult PropertyFTPStart()
        {
            _rightmove.Kill();
            _rightmove.RunUpdate(HttpContext);
            return Json(new { success = true });
        }

        [HttpPost]
        [Route("admin/property/import/rightmove/cancel/")]
        public IActionResult PropertyFTPCancel()
        {
            _rightmove.Kill();
            return Json(new { success = true });
        }

        [Route("admin/property/import/rightmove/status/")]
        public IActionResult PropertyFTPStatus()
        {
            return Json(new
            {
                Importer = _rightmove.Report(),
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
