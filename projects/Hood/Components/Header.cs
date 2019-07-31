using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using Hood.ViewModels;
using System.Threading.Tasks;

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

        public async Task<IViewComponentResult> InvokeAsync()
        {
            HeaderModel model = new HeaderModel()
            {
                Pages = await _content.GetPages()
            };
            return View(model);
        }
    }
}
