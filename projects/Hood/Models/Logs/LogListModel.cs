using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{

    public class LogListModel : PagedList<Log>, IPageableModel
    {
        public LogListModel()
        {
            PageSize = 20;
            PageIndex = 1;
        }

        public string Order { get; set; }
        public string Search { get; set; }

        [FromQuery(Name = "userId")]
        public string UserId { get; set; }
        [FromQuery(Name = "entityId")]
        public string EntityId { get; set; }

        public string GetPageUrl(int pageIndex)
        {
            var query = string.Format("?page={0}&pageSize={1}", pageIndex, PageSize);
            query += Search.IsSet() ? "&search=" + Search : "";
            query += EntityId.IsSet() ? "&entityId=" + EntityId : "";
            query += Order.IsSet() ? "&sort=" + Order : "";
            query += UserId.IsSet() ? "&userId=" + UserId : "";
            return query;
        }
    }
}
