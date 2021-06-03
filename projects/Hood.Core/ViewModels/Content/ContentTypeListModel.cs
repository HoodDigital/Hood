using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class ContentTypeListModel : PagedList<ContentType>, IPageableModel
    {
        [FromQuery(Name = "active")]
        [Display(Name = "Hide Disabled?", Description = "Check this to only show the active content types on the site")]
        public bool HideDisabled { get; set; }

        public ContentTypeListModel() : base()
        {
        }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            return query;
        }

    }
}