using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.Models
{
    public class ApiKeyModel : PagedList<ApiKey>, IPageableModel
    {
        // Params
        [FromQuery(Name = "sort")]
        public string Order { get; set; }
        [FromQuery(Name = "search")]
        public string Search { get; set; }
        [FromQuery(Name = "owner")]
        public string OwnerName { get; set; }

        public string GetPageUrl(int pageIndex)
        {
            var query = string.Format("?page={0}&pageSize={1}", pageIndex, PageSize);
            query += Search.IsSet() ? "&search=" + Search : "";
            query += Order.IsSet() ? "&sort=" + Order : "";
            query += OwnerName.IsSet() ? "&owner=" + OwnerName : "";
            return query;
        }

    }
}
