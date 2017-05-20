using Hood.Services;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using Hood.Extensions;

namespace Hood.ViewComponents
{
    public class Header : ViewComponent
    {
        private readonly IContentRepository _content;
        private readonly IShoppingCart _cart;

        public Header(IContentRepository content, IShoppingCart cart)
        {
            _content = content;
            _cart = cart;
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
