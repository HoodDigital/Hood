using System;
using System.IO;
using System.Threading.Tasks;
using Hood.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Hood.Services
{
    public class RazorViewRenderer : IRazorViewRenderer
    {
        private IRazorViewEngine _viewEngine;
        private ITempDataProvider _tempDataProvider;
        private IServiceProvider _serviceProvider;

        public RazorViewRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> Render(string viewPath)
        {
            return await Render(viewPath, string.Empty);
        }

        public async Task<string> Render<TModel>(string name, TModel model)
        {
            var actionContext = GetActionContext();
            if (!name.StartsWith("~"))
                name = "~" + (name.StartsWith("/") ? name : "/" + name);
            var viewEngineResult = _viewEngine.GetView("", name, false);

            if (!viewEngineResult.Success)
            {
                throw new InvalidOperationException(string.Format("Couldn't find view '{0}'", name));
            }

            var view = viewEngineResult.View;

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                await view.RenderAsync(viewContext);

                return output.ToString();
            }
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext()
            {
                RequestServices = Engine.Services.Resolve<IServiceProvider>()
            };
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}