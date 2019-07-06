using Hood.Core;
using Hood.Entities;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "File Size (bytes)")]
        public long FileSize { get; set; }
        [Display(Name = "File Type (mime)")]
        public string FileType { get; set; }
        [Display(Name = "Filename")]
        public string Filename { get; set; }
        [Display(Name = "Directory")]
        public string Directory { get; set; }
        [Display(Name = "Blob Reference")]
        public string BlobReference { get; set; }
        [Display(Name = "Url")]
        public string Url { get; set; }
        [Display(Name = "Uploaded On")]
        public DateTime CreatedOn { get; set; }
        [Display(Name = "Thumbnail Url", Description = "Large URL for the file (250x250 Max Size)")]
        public string ThumbUrl { get; set; }
        [Display(Name = "Small Url", Description = "Large URL for the file (640x640 Max Size)")]
        public string SmallUrl { get; set; }
        [Display(Name = "Medium Url", Description = "Large URL for the file (1280x1280 Max Size)")]
        public string MediumUrl { get; set; }
        [Display(Name = "Large Url", Description = "Large URL for the file (1920x1920 Max Size)")]
        public string LargeUrl { get; set; }
        [Display(Name = "Unique Id", Description = "Unique file reference for the filename generation.")]
        public string UniqueId { get; set; }
        public GenericFileType GenericFileType { get; set; }

        public virtual string Container
        {
            get
            {
                if (GenericFileType == GenericFileType.Directory)
                    return "N/A";
                return Directory;
            }
        }
        [Display(Name = "Url")]
        public virtual string DownloadUrl => Url.Replace("https://", "http://");
        [Display(Name = "Secure Url")]
        public virtual string DownloadUrlHttps => Url.Replace("http://", "https://");
        [Display(Name = "Icon")]
        public virtual string Icon => this.ToIcon();
        [Display(Name = "File Size (Kb)")]
        public virtual string FormattedSize => (FileSize / 1024).ToString() + "Kb";

        public static string NoImageUrl
        {
            get
            {
                string noImage;
                try
                {
                    var mediaSettings = Engine.Settings.Media;
                    noImage = mediaSettings.NoImage.IsSet() ? mediaSettings.NoImage : "/hood/images/no-image.jpg";
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
                string noImage;
                try
                {
                    var mediaSettings = Engine.Settings.Media;
                    noImage = mediaSettings.NoImage.IsSet() ? mediaSettings.NoImage : "/hood/images/no-image.jpg";
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

        /// <summary>
        /// Please use IMediaObject.GenericFileType instead.
        /// </summary>
        [Obsolete("Please use IMediaObject.GenericFileType instead.", true)]
        public string GeneralFileType { get; set; }
    }
}
