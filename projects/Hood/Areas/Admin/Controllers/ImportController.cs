using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hood.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor,Manager")]
    public class ImportController : Controller
    {
        private IPropertyImporter _propertyImporterFTP;
        private IPropertyExporter _propertyExporter;
        private IContentExporter _contentExporter;
        private IFTPService _ftp;

        public ImportController(IFTPService ftp, IPropertyImporter propertyImporterFTP, IContentExporter contentExporter, IPropertyExporter propertyExporter)
        {
            _ftp = ftp;
            _propertyImporterFTP = propertyImporterFTP;
            _contentExporter = contentExporter;
            _propertyExporter = propertyExporter;
        }

        #region "FTPPropertyImporter"

        [Route("admin/property/import/ftp/")]
        public IActionResult PropertyFTP()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/property/import/ftp/start/")]
        public IActionResult PropertyFTPStart()
        {
            _propertyImporterFTP.Kill();
            _propertyImporterFTP.RunUpdate(HttpContext);
            return Json(new { success = true });
        }

        [HttpPost]
        [Route("admin/property/import/ftp/cancel/")]
        public IActionResult PropertyFTPCancel()
        {
            _propertyImporterFTP.Kill();
            return Json(new { success = true });
        }

        [Route("admin/property/import/ftp/status/")]
        public IActionResult PropertyFTPStatus()
        {
            return Json(new
            {
                Importer = _propertyImporterFTP.Report(),
                Ftp = _ftp.Report()
            });
        }

        #endregion

        #region "Content"

        [Route("admin/content/export/")]
        public IActionResult ContentExport()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/content/export/start/")]
        public IActionResult ContentExportStart()
        {
            _contentExporter.Kill();
            _contentExporter.ExportContent(HttpContext);
            return Json(new { success = true });
        }

        [HttpPost]
        [Route("admin/content/export/cancel/")]
        public IActionResult ContentExportCancel()
        {
            _contentExporter.Kill();
            return Json(new { success = true });
        }

        [Route("admin/content/export/status/")]
        public IActionResult ContentExportStatus()
        {
            return Json(_contentExporter.Report());
        }

        #endregion

        #region "Properties"

        [Route("admin/properties/export/")]
        public IActionResult PropertiesExport()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/properties/export/start/")]
        public IActionResult PropertiesExportStart()
        {
            _propertyExporter.Kill();
            _propertyExporter.ExportProperties(HttpContext);
            return Json(new { success = true });
        }

        [HttpPost]
        [Route("admin/properties/export/cancel/")]
        public IActionResult PropertiesExportCancel()
        {
            _propertyExporter.Kill();
            return Json(new { success = true });
        }

        [Route("admin/properties/export/status/")]
        public IActionResult PropertiesExportStatus()
        {
            return Json(_propertyExporter.Report());
        }

        #endregion


    }
}
