using Hood.Models.Api;
using System.Collections.Generic;

namespace Hood.Models
{
    public class ShopSidebarModel
    {
        public IList<ContentApi> Recent { get; set; }
        public string Search { get; set; }
    }
}
