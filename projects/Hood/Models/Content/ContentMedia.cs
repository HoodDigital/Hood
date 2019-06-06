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
    public class ContentMedia : MediaObjectBase
    {
        public ContentMedia() : base()
        { }

        public ContentMedia(IMediaObject mediaResult) : base(mediaResult)
        { }

        public int ContentId { get; set; }
        public Content Content { get; set; }

        public new static IMediaObject Blank => MediaObjectBase.Blank;
    }
}
