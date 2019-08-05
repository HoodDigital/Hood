using Hood.Services;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class ThemeListView : PagedList<Theme>
    {
        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            return query;
        }
    }
}