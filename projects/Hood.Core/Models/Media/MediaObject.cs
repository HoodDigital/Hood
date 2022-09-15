using Hood.Core;
using Hood.Entities;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public sealed class MediaObject : MediaBase
    {
        #region Constructors
        public MediaObject()
        {
        }

        public MediaObject(IMediaObject mediaResult, int? directoryId = null) 
            : base(mediaResult)
        {
            DirectoryId = directoryId;
        }

        public MediaObject(string url, string smallUrl = null, string mediumUrl = null, string largeUrl = null, string thumbUrl = null)
            : base(url, smallUrl, mediumUrl, largeUrl, thumbUrl)
        {
        }
        #endregion

        #region Directory
        [JsonIgnore]
        [Display(Name = "Directory")]
        public MediaDirectory Directory { get; set; }
        [Display(Name = "Directory")]
        public int? DirectoryId { get; set; }

        [Display(Name = "Directory Path", Description = "The directory path for this file in the chosen storage location.")]
        public override string Path { get; set; }
        #endregion
    }

    public abstract class MediaBase : BaseEntity, IMediaObject
    {
        #region Constructors
        public MediaBase()
        { }
        public MediaBase(IMediaObject mediaResult)
        {
            mediaResult.CopyProperties(this);
        }
        public MediaBase(string url, string smallUrl = null, string mediumUrl = null, string largeUrl = null, string thumbUrl = null)
        {
            ThumbUrl = thumbUrl.IsSet() ? thumbUrl : url;
            SmallUrl = smallUrl.IsSet() ? thumbUrl : url;
            MediumUrl = mediumUrl.IsSet() ? thumbUrl : url;
            LargeUrl = largeUrl.IsSet() ? thumbUrl : url;
            Url = url;
        }

        #endregion

        #region Properties
        [Display(Name = "File Size (bytes)")]
        public virtual long FileSize { get; set; }
        [Display(Name = "File Type (mime)")]
        public virtual string FileType { get; set; }
        [Display(Name = "Filename")]
        public virtual string Filename { get; set; }
        [Display(Name = "Blob Reference")]
        public virtual string BlobReference { get; set; }
        [Display(Name = "Url")]
        public virtual string Url { get; set; }
        [Display(Name = "Uploaded On")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddThh:mm}")]
        public virtual DateTime CreatedOn { get; set; }
        [Display(Name = "Created By")]
        public virtual string CreatedBy { get; set; }
        [Display(Name = "Thumbnail Url", Description = "Large URL for the file (250x250 Max Size)")]
        public virtual string ThumbUrl { get; set; }
        [Display(Name = "Small Url", Description = "Large URL for the file (640x640 Max Size)")]
        public virtual string SmallUrl { get; set; }
        [Display(Name = "Medium Url", Description = "Large URL for the file (1280x1280 Max Size)")]
        public virtual string MediumUrl { get; set; }
        [Display(Name = "Large Url", Description = "Large URL for the file (1920x1920 Max Size)")]
        public virtual string LargeUrl { get; set; }
        [Display(Name = "Unique Id", Description = "Unique file reference for the filename generation.")]
        public virtual string UniqueId { get; set; }
        [Display(Name = "Directory Path", Description = "The directory path for this file in the chosen storage location.")]
        public virtual string Path { get; set; }
        [Display(Name = "Generic File Type", Description = "The general file type for this file.")]
        public virtual GenericFileType GenericFileType { get; set; }
        #endregion

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
                    MediaSettings mediaSettings = Engine.Settings.Media;
                    noImage = mediaSettings.NoImage.IsSet() ? mediaSettings.NoImage : Engine.Resource("/images/no-image.jpg");
                }
                catch
                {
                    noImage = Engine.Resource("/images/no-image.jpg");
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
                    MediaSettings mediaSettings = Engine.Settings.Media;
                    noImage = mediaSettings.NoImage.IsSet() ? mediaSettings.NoImage : Engine.Resource("/images/no-image.jpg");
                }
                catch
                {
                    noImage = Engine.Resource("/images/no-image.jpg");
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
        public static IMediaObject BlankAvatar
        {
            get
            {
                string noImage;
                try
                {
                    MediaSettings mediaSettings = Engine.Settings.Media;
                    noImage = mediaSettings.NoImage.IsSet() ? mediaSettings.NoImage : Engine.Resource("/images/no-avatar.jpg");
                }
                catch
                {
                    noImage = Engine.Resource("/images/no-avatar.jpg");
                }
                MediaObject ret = new MediaObject
                {
                    FileSize = 0,
                    Filename = "no-avatar.jpg",
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
