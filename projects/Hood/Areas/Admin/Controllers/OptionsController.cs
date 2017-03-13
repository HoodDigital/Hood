using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Hood.Services;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Hood.Api
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    public class OptionsController : Controller
    {
        private readonly ISettingsRepository _options;
        public OptionsController(ISettingsRepository options)
        {
            _options = options;
        }

        [HttpGet]
        public JsonResult Get(ListFilters request, string search, string sort, string type)
        {
            IList<Option> options = null;
            switch (type)
            {
                default:
                    options = _options.AllSettings();
                    break;
            }
            if (!string.IsNullOrEmpty(search))
            {
                string[] searchTerms = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                options = options.Where(n => searchTerms.Any(s => n.Id.ToLower().Contains(s.ToLower()))).ToList();
            }

            switch (sort)
            {
                case "Key":
                    options = options.OrderBy(n => n.Id).ToList();
                    break;
                case "Value":
                    options = options.OrderBy(n => n.Value).ToList();
                    break;

                case "KeyDesc":
                    options = options.OrderByDescending(n => n.Id).ToList();
                    break;
                case "ValueDesc":
                    options = options.OrderByDescending(n => n.Value).ToList();
                    break;

                default:
                    options = options.OrderBy(n => n.Id).ToList();
                    break;
            }

            Response response = new Response(options.Skip(request.skip).Take(request.take).ToArray(), options.Count());
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            return Json(response, settings);
        }

        [HttpGet]
        public JsonResult GetById(string Id)
        {
            return Json(_options.Get<string>(Id));
        }

        [HttpPost()]
        public Response Set(string name, string value)
        {
            try
            {
                _options[name] = value;
                return new Response(true);
            }
            catch (Exception ex)
            {
                // return any issue via the Json Response with errors.
                return new Response(ex.Message);
            }
        }


        [HttpPost()]
        public Response Update(List<Option> models)
        {
            try
            {
                foreach (Option opt in models)
                {
                    _options.Set(opt.Id, opt.Value);
                }
                return new Response(true);
            }
            catch (Exception ex)
            {
                // return any issue via the Json Response with errors.
                return new Response(ex.Message);
            }
        }

        [HttpPost()]
        public Response Add(List<Option> models)
        {
            try
            {
                foreach (Option opt in models)
                {
                    _options.Set(opt.Id, opt.Value);
                }
                return new Response(true);
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [HttpPost()]
        public Response Delete(List<Option> models)
        {
            try
            {
                foreach (Option opt in models)
                {
                    _options.Delete(opt.Id);
                }
                return new Response(true);
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

    }
}
