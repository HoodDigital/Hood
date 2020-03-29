using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Hood.Controllers
{
    [Route("error")]
    public class ErrorController : BaseController
    {

        public ErrorController() : base()
        { }

        [Route("500")]
        public async System.Threading.Tasks.Task<IActionResult> AppError()
        {
            ErrorModel model = GetErrorInformation();

            model.OriginalUrl = HttpContext.GetSiteUrl().TrimEnd('/');
            if (HttpContext.Items.ContainsKey("originalPath"))
            {
                model.OriginalUrl += HttpContext.Items["originalPath"] as string;
            }

            await _logService.AddExceptionAsync<ErrorController>($"500 - Application Error: {model.OriginalUrl}", model.Error);

            return View("Index", model);
        }

        [Route("404")]
        public async System.Threading.Tasks.Task<IActionResult> PageNotFound()
        {
            BasicSettings basicSettings = Engine.Settings.Basic;
            if (basicSettings.LockoutMode && ControllerContext.HttpContext.IsLockedOut(Engine.Settings.LockoutAccessCodes))
            {
                return RedirectToActionPreserveMethod(nameof(HomeController.Index), "Home");
            }

            ErrorModel model = new ErrorModel
            {
                OriginalUrl = "unknown",
                Code = 404
            };

            model.OriginalUrl = HttpContext.GetSiteUrl().TrimEnd('/');
            if (HttpContext.Items.ContainsKey("originalPath"))
            {
                model.OriginalUrl += HttpContext.Items["originalPath"] as string;
            }

            await _logService.AddLogAsync<ErrorController>($"404 - Page not found: {model.OriginalUrl}", type: LogType.Error404);

            return View("Index", model);
        }

        private ErrorModel GetErrorInformation()
        {
            var model = new ErrorModel();
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (feature != null && feature.Error != null)
            {
                model.Error = feature.Error;
                model.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
                model.ErrorMessage = feature.Error.Message;
            }
            model.Code = 500;
            return model;
        }
    }
}
