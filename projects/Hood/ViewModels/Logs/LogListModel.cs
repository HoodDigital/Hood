using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
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
        [FromQuery(Name = "logType")]
        public LogType? LogType { get; set; }
        [FromQuery(Name = "source")]
        public string Source { get; set; }

        public string GetPageUrl(int pageIndex)
        {
            var query = string.Format("?page={0}&pageSize={1}", pageIndex, PageSize);
            query += Search.IsSet() ? "&search=" + Search : "";
            query += Source.IsSet() ? "&source=" + Source : "";
            query += LogType.HasValue ? "&logType=" + LogType : "";
            query += Order.IsSet() ? "&sort=" + Order : "";
            query += UserId.IsSet() ? "&userId=" + UserId : "";
            return query;
        }
    }
}
