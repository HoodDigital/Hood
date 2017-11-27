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
    public partial class PropertyMedia : PropertyMediaBase
    {
        public PropertyListing Property { get; internal set; }
        public int PropertyId { get; internal set; }

        public PropertyMedia()
            : base()
        { }

        public PropertyMedia(IMediaObject media)
            : base(media)
        { }
    }
    public partial class PropertyFloorplan : PropertyMediaBase
    {
        public PropertyListing Property { get; internal set; }
        public int PropertyId { get; internal set; }

        public PropertyFloorplan()
            : base()
        { }

        public PropertyFloorplan(IMediaObject media)
            : base(media)
        { }
    }

    public partial class PropertyMediaBase : IMediaObject
    {
        public PropertyMediaBase()
        {
        }

        public PropertyMediaBase(IMediaObject media)
        {
            FileSize = media.FileSize;
            FileType = media.FileType;
            Filename = media.Filename;
            Directory = media.Directory;
            BlobReference = media.BlobReference;
            Url = media.Url;
            CreatedOn = media.CreatedOn;
            GeneralFileType = media.GeneralFileType;
            ThumbUrl = media.ThumbUrl;
            SmallUrl = media.SmallUrl;
            MediumUrl = media.MediumUrl;
            LargeUrl = media.LargeUrl;
            UniqueId = media.UniqueId;
        }

        public int Id { get; set; }
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

        [NotMapped]
        public virtual string DownloadUrl => Url.Replace("https://", "http://");
        [NotMapped]
        public virtual string DownloadUrlHttps => Url.Replace("http://", "https://");
        [NotMapped]
        public virtual string Icon
        {
            get
            {
                GenericFileType type = GenericFileType.Unknown;
                string output = "";
                if (Enum.TryParse(GeneralFileType, out type))
                {
                    switch (type)
                    {
                        case GenericFileType.Image:
                            output = SmallUrl;
                            break;
                        case GenericFileType.Excel:
                            return "/lib/hood/images/icons/excel.png";
                        case GenericFileType.PDF:
                            return "/lib/hood/images/icons/pdf.png";
                        case GenericFileType.PowerPoint:
                            return "/lib/hood/images/icons/powerpoint.png";
                        case GenericFileType.Word:
                            return "/lib/hood/images/icons/word.png";
                        case GenericFileType.Photoshop:
                            return "/lib/hood/images/icons/photoshop.png";
                        case GenericFileType.Unknown:
                        default:
                            return "/lib/hood/images/icons/file.png";
                    }
                }
                if (!output.IsSet())
                {
                    output = NoImageUrl;
                }
                return output;
            }
        }
        [NotMapped]
        public static string NoImageUrl
        {
            get
            {
                var output = "";
                var siteSettings = EngineContext.Current.Resolve<ISettingsRepository>();
                if (siteSettings != null)
                {
                    var mediaSettings = siteSettings.GetMediaSettings();
                    output = mediaSettings.NoImage.IsSet() ? mediaSettings.NoImage : "/lib/hood/images/no-image.jpg";
                }
                return output;
            }
        }
        [NotMapped]
        public virtual string FormattedSize => (FileSize / 1024).ToString() + "Kb";

        public static IMediaObject Blank
        {
            get
            {
                var noImg = NoImageUrl;
                MediaObject ret = new MediaObject
                {
                    FileSize = 0,
                    Filename = "no-image.jpg",
                    SmallUrl = noImg,
                    MediumUrl = noImg,
                    LargeUrl = noImg,
                    ThumbUrl = noImg,
                    Url = noImg,
                    BlobReference = "N/A"
                };
                return ret;
            }
        }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
