using Hood.Core;
using Hood.BaseControllers;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Hood.Admin.BaseControllers
{
    public abstract class BaseImportController : BaseController
    {
        private readonly IFTPService _ftp;
        private readonly IPropertyImporter _blm;

        public BaseImportController()
            : base()
        {
            _ftp = Engine.Services.Resolve<IFTPService>();
            _blm = Engine.Services.Resolve<IPropertyImporter>();
        }

        #region "BlmPropertyImporter"

        [Route("admin/property/import/blm/trigger")]
        [AllowAnonymous]
        public virtual async Task<IActionResult> BlmPropertyImporterTrigger()
        {
            var triggerAuth = Engine.Settings.Property.TriggerAuthKey;
            if (Request.Headers.ContainsKey("Auth") && Request.Headers["Auth"] == triggerAuth && !_blm.IsRunning())
            {
               await _blm.RunUpdate(HttpContext);
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

            await _logService.AddLogAsync<BaseImportController>("Unauthorized API access attempt.", logWriter.ToString(), LogType.Warning);

            return StatusCode(401);
        }

        [Route("admin/property/import/blm/")]
        public virtual IActionResult BlmImporter()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/property/import/blm/start/")]
        public virtual IActionResult BlmImporterStart()
        {
            _blm.Kill();
            _blm.RunUpdate(HttpContext);
            return Json(new { success = true });
        }

        [HttpPost]
        [Route("admin/property/import/blm/cancel/")]
        public virtual IActionResult BlmImporterCancel()
        {
            _blm.Kill();
            return Json(new { success = true });
        }

        [Route("admin/property/import/blm/status/")]
        public virtual IActionResult BlmImporterStatus()
        {
            return Json(new
            {
                Importer = _blm.Report(),
                Ftp = _ftp.Report()
            });
        }

        #endregion

    }
}
