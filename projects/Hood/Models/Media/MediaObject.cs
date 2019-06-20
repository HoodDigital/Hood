using Hood.Core;
using Hood.Entities;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using Newtonsoft.Json;
using System;

namespace Hood.Models
{
    public sealed class MediaObject : MediaObjectBase
    {
        public MediaObject() : base()
        {}

        public MediaObject(IMediaObject mediaResult) : base(mediaResult)
        { }

        public MediaObject(string url, string smallUrl = null, string mediumUrl = null, string largeUrl = null, string thumbUrl = null) : base(url, smallUrl, mediumUrl, largeUrl, thumbUrl)
        { }

        public new static IMediaObject Blank => MediaObjectBase.Blank;
    }

    public abstract class MediaObjectBase : BaseEntity, IMediaObject
    {
        public MediaObjectBase()
        {}

        public MediaObjectBase(string url, string smallUrl = null, string mediumUrl = null, string largeUrl = null, string thumbUrl = null)
        {
            ThumbUrl = thumbUrl.IsSet() ? thumbUrl : url;
            SmallUrl = smallUrl.IsSet() ? thumbUrl : url;
            MediumUrl = mediumUrl.IsSet() ? thumbUrl : url;
            LargeUrl = largeUrl.IsSet() ? thumbUrl : url;
            Url = url;
        }

        public MediaObjectBase(IMediaObject mediaResult)
        {
            mediaResult.CopyProperties(this);
        }

        public long FileSize { get; set; }
        public string FileType { get; set; }
        public string Filename { get; set; }
        public string Directory { get; set; }
        public string BlobReference { get; set; }
        public string Url { get; set; }
        public DateTime CreatedOn { get; set; }
        public string GeneralFileType { get; set; }
        public string ThumbUrl { get; set; }
        public string SmallUrl { get; set; }
        public string MediumUrl { get; set; }
        public string LargeUrl { get; set; }
        public string UniqueId { get; set; }

        public virtual string Container
        {
            get
            {
                if (GeneralFileType == "Directory")
                    return "N/A";
                return Directory;
            }
        }
        public virtual string DownloadUrl => Url.Replace("https://", "http://");
        public virtual string DownloadUrlHttps => Url.Replace("http://", "https://");
        public virtual string Icon => this.ToIcon();
        public virtual string FormattedSize => (FileSize / 1024).ToString() + "Kb";
        public virtual GenericFileType GenericFileType => FileType.ToFileType();

        public static string NoImageUrl
        {
            get
            {
                var noImage = "";
                try
                {
                    var siteSettings = Engine.Current.Resolve<ISettingsRepository>();
                    if (siteSettings != null)
                    {
                        var mediaSettings = siteSettings.GetMediaSettings();
                        noImage = mediaSettings.NoImage.IsSet() ? mediaSettings.NoImage : "/hood/images/no-image.jpg";
                    }
                }
                catch
                {
                    noImage = "/hood/images/no-image.jpg";
                }
                return noImage;
            }
        }
        public static IMediaObject Blank
        {
            get
            {
                var noImage = "";
                try
                {
                    var siteSettings = Engine.Current.Resolve<ISettingsRepository>();
                    if (siteSettings != null)
                    {
                        var mediaSettings = siteSettings.GetMediaSettings();
                        noImage = mediaSettings.NoImage.IsSet() ? mediaSettings.NoImage : "/hood/images/no-image.jpg";
                    }
                }
                catch
                {
                    noImage = "/hood/images/no-image.jpg";
                }
                MediaObject ret = new MediaObject
                {
                    FileSize = 0,
                    Filename = "no-image.jpg",
                    SmallUrl = noImage,
                    MediumUrl = noImage,
                    LargeUrl = noImage,
                    ThumbUrl = noImage,
                    Url = noImage,
                    BlobReference = "N/A"
                };
                return ret;
            }
        }
    }
}
