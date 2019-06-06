using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public class ForumMedia : MediaObjectBase
    {
        public ForumMedia()
            : base()
        { }

        public ForumMedia(IMediaObject media)
            : base(media)
        { }
        public new static IMediaObject Blank => MediaObjectBase.Blank;

        public int ForumId { get; set; }
        public Forum Forum { get; set; }
    }
}
