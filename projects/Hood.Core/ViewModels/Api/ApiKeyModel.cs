using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class ApiKeyModel : PagedList<ApiKey>, IPageableModel
    {
        // Params
        [FromQuery(Name = "owner")]
        public string OwnerName { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            query += OwnerName.IsSet() ? "&owner=" + OwnerName : "";
            return query;
        }

    }
}
