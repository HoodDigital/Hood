using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Infrastructure
{
    /// <summary>
    /// This will allow you to map subdomains to areas in the app, eg admin.yourdomain.com will map to the Admin area. Not in use currently.
    /// </summary>
    public class AreaRouter : MvcRouteHandler, IRouter
    {
        private string[] _allowedSubdomains = { "admin", "password", "login" };
        //These are actualy copies of same values from base class. Some of them are used later.
        private IActionContextAccessor _actionContextAccessor;
        private IActionInvokerFactory _actionInvokerFactory;
        private IActionSelector _actionSelector;
        private ILogger _logger;
        private DiagnosticSource _diagnosticSource;

        public AreaRouter(
            IActionInvokerFactory actionInvokerFactory,
            IActionSelector actionSelector,
            DiagnosticSource diagnosticSource,
            ILoggerFactory loggerFactory)
            : this(actionInvokerFactory, actionSelector, diagnosticSource, loggerFactory, actionContextAccessor: null)
        {
        }

        public AreaRouter(IActionInvokerFactory actionInvokerFactory, IActionSelector actionSelector, DiagnosticSource diagnosticSource,
            ILoggerFactory loggerFactory, IActionContextAccessor actionContextAccessor)
            : base(actionInvokerFactory, actionSelector, diagnosticSource,
            loggerFactory, actionContextAccessor)
        {
            _actionContextAccessor = actionContextAccessor;
            _actionInvokerFactory = actionInvokerFactory;
            _actionSelector = actionSelector;
            _diagnosticSource = diagnosticSource;
            _logger = loggerFactory.CreateLogger<MvcRouteHandler>();
        }

        public new Task RouteAsync(RouteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            string url = context.HttpContext.Request.Headers["HOST"];
            string firstDomain = url.Split('.')[0];
            //Areas usually start from uooer-case letter.
            string subDomain = firstDomain.ToLower();
            //check if our app knows subdomain
            if (_allowedSubdomains.Contains(subDomain))
                context.RouteData.Values.Add("area", subDomain);

            //All the next code is copied from base class
            var candidates = _actionSelector.SelectCandidates(context);
            if (candidates == null || candidates.Count == 0)
            {
                return Task.CompletedTask;
            }

            var actionDescriptor = _actionSelector.SelectBestCandidate(context, candidates);
            if (actionDescriptor == null)
            {
                return Task.CompletedTask;
            }

            context.Handler = (c) =>
            {
                var routeData = c.GetRouteData();

                var actionContext = new ActionContext(context.HttpContext, routeData, actionDescriptor);
                if (_actionContextAccessor != null)
                {
                    _actionContextAccessor.ActionContext = actionContext;
                }

                var invoker = _actionInvokerFactory.CreateInvoker(actionContext);
                if (invoker == null)
                {
                    throw new InvalidOperationException();
                }

                return invoker.InvokeAsync();
            };

            return Task.CompletedTask;
        }

    }
}