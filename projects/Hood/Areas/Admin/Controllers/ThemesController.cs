using System;
using Microsoft.AspNetCore.Mvc;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Hood.Services;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Hood.Api
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    public class ThemesController : Controller
    {
        private readonly ISiteConfiguration _options;
        public ThemesController(ISiteConfiguration options)
        {
            _options = options;
        }

        [HttpPost()]
        public Response Activate(string name)
        {
            try
            {
                // set the site theme
                bool res = _options.Set("Hood.Settings.Theme", name);
                if (res)
                {
                    return new Response(true);
                }
                else
                {
                    throw new Exception("The database could not be updated, please try later.");
                }
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

    }
}
