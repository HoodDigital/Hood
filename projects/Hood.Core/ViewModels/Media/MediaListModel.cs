using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class MediaListModel : PagedList<MediaObject>, IPageableModel
    {
        public MediaListModel()
        {
            PageSize = 20;
            PageIndex = 1;
        }

        [FromQuery(Name = "user")]
        public string UserId { get; set; }
        [FromQuery(Name = "fileType")]
        public GenericFileType? GenericFileType { get; set; }
        [FromQuery(Name = "restrict")]
        public bool Restrict { get; set; }
        [FromQuery(Name = "dir")]
        public int? DirectoryId { get; set; }
        [FromQuery(Name = "root")]
        public int? RootId { get; set; }

        #region Actions
        [FromQuery(Name = "doAction")]
        public MediaWindowAction? Action { get; set; }
        [FromQuery(Name = "id")]
        public string Id { get; set; }
        [FromQuery(Name = "entity")]
        public string Entity { get; set; }
        [FromQuery(Name = "field")]
        public string Field { get; set; }
        [FromQuery(Name = "type")]
        public string Type { get; set; }
        [FromQuery(Name = "tag")]
        public string Tag { get; set; }
        [FromQuery(Name = "media")]
        public int? MediaId { get; set; }
        #endregion

        public IEnumerable<MediaDirectory> TopLevelDirectories { get; set; }
        public MediaDirectory Root { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);

            query += Action.HasValue ? "&doAction=" + Action : "";
            query += DirectoryId.HasValue ? "&dir=" + DirectoryId : "";
            query += RootId.HasValue ? "&root=" + RootId : "";
            query += Restrict ? "&restrict=" + Restrict : "";
            query += GenericFileType.HasValue ? "&fileType=" + GenericFileType : "";
            query += UserId.IsSet() ? "&user=" + UserId : "";


            query += Id.IsSet() ? "&id=" + Id : "";
            query += Entity.IsSet() ? "&entity=" + Entity : "";
            query += Field.IsSet() ? "&field=" + Field : "";
            query += Type.IsSet() ? "&type=" + Type : "";
            query += MediaId.HasValue ? "&media=" + MediaId : "";

            return query;
        }
    }
}