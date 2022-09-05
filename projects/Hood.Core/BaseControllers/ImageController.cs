using System;
using System.Threading.Tasks;
using Hood.Core;
using Hood.Extensions;
using Hood.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Unsplasharp;

namespace Hood.BaseControllers
{
    public class ImagesController : Controller
    {

        public ImagesController()
        { }


        #region Login Page Backgrounds 

        [HttpGet]
        [Route("hood/images/random/{query?}")]
        public virtual async Task<IActionResult> BackgroundImage(string query)
        {
            try
            {
                if (Engine.Settings.Integrations.UnsplashAccessKey.IsSet())
                {
                    var client = new UnsplasharpClient(Engine.Settings.Integrations.UnsplashAccessKey);
                    var photosFound = await client.GetRandomPhoto(UnsplasharpClient.Orientation.Squarish, query: query);
                    return Json(photosFound);
                }
                else
                {
                    return Content(Engine.Settings.Basic.LoginAreaSettings.BackgroundImages.Split(Environment.NewLine).PickRandom());
                }
            }
            catch
            {
                return Content("https://source.unsplash.com/random");
            }
        }

        #endregion

    }
}
