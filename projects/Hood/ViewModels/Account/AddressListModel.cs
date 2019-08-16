using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class AddressListModel : PagedList<Address>, IPageableModel
    {        // Params
        [FromQuery(Name = "user")]
        public string UserId { get; set; }
        public UserProfile UserProfile { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            query += UserId.IsSet() ? "&user=" + UserId : "";
            return query;
        }

    }
}