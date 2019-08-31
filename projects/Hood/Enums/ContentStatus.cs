using Microsoft.AspNetCore.Html;

namespace Hood.Enums
{

    public enum ContentStatus
    {
        Private = 0,
        Draft = 1,
        Published = 2,
        Archived = 3,
        Deleted = 4,
        Revision = 5
    }
    public static class ContentStatusExtensions
    {
        public static IHtmlContent ToHtml(this ContentStatus status)
        {
            switch (status)
            {
                case ContentStatus.Published:
                    return new HtmlString($"<span class='badge badge-pill badge-success'>{status.ToString()}</span>");
                case ContentStatus.Draft:
                    return new HtmlString($"<span class='badge badge-pill badge-warning'>{status.ToString()}</span>");
                case ContentStatus.Archived:
                    return new HtmlString($"<span class='badge badge-pill badge-info'>{status.ToString()}</span>");
                case ContentStatus.Private:
                    return new HtmlString($"<span class='badge badge-pill badge-dark'><i class='fa fa-lock mr-2'></i>{status.ToString()}</span>");
                case ContentStatus.Deleted:
                    return new HtmlString($"<span class='badge badge-pill badge-danger'>{status.ToString()}</span>");
                default:
                    return new HtmlString($"<span class='badge badge-pill badge-light'>{status.ToString()}</span>");
            }
        }
    }

}
