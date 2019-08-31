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

        [FromQuery(Name = "userId")]
        public string UserId { get; set; }
        [FromQuery(Name = "logType")]
        public LogType? LogType { get; set; }
        [FromQuery(Name = "source")]
        public string Source { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            query += Source.IsSet() ? "&source=" + Source : "";
            query += LogType.HasValue ? "&logType=" + LogType : "";
            query += UserId.IsSet() ? "&userId=" + UserId : "";
            return query;
        }
    }
}
