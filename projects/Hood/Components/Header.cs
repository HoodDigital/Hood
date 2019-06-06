using Hood.Services;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using Hood.Extensions;

namespace Hood.ViewComponents
{
    [ViewComponent(Name = "Header")]
    public class Header : ViewComponent
    {
        private readonly IContentRepository _content;

        public Header(IContentRepository content)
        {
            _content = content;
        }

        public IViewComponentResult Invoke()
        {
            HeaderModel model = new HeaderModel()
            {
                Pages = _content.GetContentByType("page"),
                Subscription = HttpContext.GetAccountInfo()
            };
            model.User = model.Subscription.User;
            return View(model);
        }
    }
}
