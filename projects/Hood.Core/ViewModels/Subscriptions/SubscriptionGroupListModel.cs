using Hood.Interfaces;
using Hood.Models;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class SubscriptionProductListModel : PagedList<SubscriptionProduct>, IPageableModel
    {
        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            return query;
        }
    }
}