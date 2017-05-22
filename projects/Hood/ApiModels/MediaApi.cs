using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Services;
using System;

namespace Hood.Models.Api
{
    public partial class MediaApi
    {
        public int Id { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
        public string Filename { get; set; }
        public string Directory { get; set; }
        public string BlobReference { get; set; }
        public string Container { get; set; }
        public string DownloadUrl { get; set; }
        public string DownloadUrlHttps { get; set; }
        public int Status { get; set; }
        public string Title { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public string UserVars { get; set; }
        public string Notes { get; set; }
        public string SystemNotes { get; set; }
        public string GeneralFileType { get; set; }
        public string UniqueId { get; set; }

        // Formatted Members
        public string Icon { get; set; }
        public string FormattedSize { get; set; }

        public string ThumbUrl { get; set; }
        public string SmallUrl { get; set; }
        public string MediumUrl { get; set; }
        public string LargeUrl { get; set; }

        public MediaApi(IMediaObject mi, ISettingsRepository settings)
        {
            if (mi == null)
                return;
            if (mi != null)
            {
                mi.CopyProperties(this);
                FormattedSize = (mi.FileSize / 1024).ToString() + "Kb";
                DownloadUrl = mi.Url.Replace("https://", "http://");
                DownloadUrlHttps = mi.Url.Replace("http://", "https://");

                if (GeneralFileType == "Directory")
                {
                    DownloadUrl = DownloadUrl + Directory.ToSeoUrl();
                    DownloadUrlHttps = DownloadUrlHttps + Directory.ToSeoUrl();
                    Container = "N/A";
                    BlobReference = "N/A";
                }

            }
            // Formatted Members

            GenericFileType type = GenericFileType.Unknown;
            if (Enum.TryParse(GeneralFileType, out type))
            {
                switch (type)
                {
                    case GenericFileType.Image:
                        Icon = mi.SmallUrl;
                        SmallUrl = mi.SmallUrl;
                        MediumUrl = mi.MediumUrl;
                        LargeUrl = mi.LargeUrl;
                        ThumbUrl = mi.ThumbUrl;
                        break;
                    case GenericFileType.Excel:
                        Icon = "/lib/hood/images/icons/excel.png";
                        SmallUrl = Icon;
                        MediumUrl = Icon;
                        LargeUrl = Icon;
                        ThumbUrl = Icon;
                        break;
                    case GenericFileType.PDF:
                        Icon = "/lib/hood/images/icons/pdf.png";
                        SmallUrl = Icon;
                        MediumUrl = Icon;
                        LargeUrl = Icon;
                        ThumbUrl = Icon;
                        break;
                    case GenericFileType.PowerPoint:
                        Icon = "/lib/hood/images/icons/powerpoint.png";
                        SmallUrl = Icon;
                        MediumUrl = Icon;
                        LargeUrl = Icon;
                        ThumbUrl = Icon;
                        break;
                    case GenericFileType.Word:
                        Icon = "/lib/hood/images/icons/word.png";
                        SmallUrl = Icon;
                        MediumUrl = Icon;
                        LargeUrl = Icon;
                        ThumbUrl = Icon;
                        break;
                    case GenericFileType.Photoshop:
                        Icon = "/lib/hood/images/icons/photoshop.png";
                        SmallUrl = Icon;
                        MediumUrl = Icon;
                        LargeUrl = Icon;
                        ThumbUrl = Icon;
                        break;
                    case GenericFileType.Unknown:
                        Icon = "/lib/hood/images/icons/file.png";
                        SmallUrl = Icon;
                        MediumUrl = Icon;
                        LargeUrl = Icon;
                        ThumbUrl = Icon;
                        break;
                }
            }

            if (string.IsNullOrEmpty(Icon))
            {
                var mediaSettings = settings.GetMediaSettings();
                string url = mediaSettings.NoImage.IsSet() ? mediaSettings.NoImage : "/lib/hood/images/no-image.jpg";
            }

        }

        public MediaApi()
        {
        }

        public static MediaApi Blank(MediaSettings settings = null)
        {
            MediaApi ret = new MediaApi()
            {

                // Formatted Members
                FormattedSize = "0Kb"
            };
            var noImage = "/lib/hood/images/no-image.jpg";
            if (settings.NoImage.IsSet())
                noImage = settings.NoImage;

            ret.Icon = noImage;
            ret.SmallUrl = noImage;
            ret.MediumUrl = noImage;
            ret.LargeUrl = noImage;
            ret.ThumbUrl = noImage;
            ret.DownloadUrl = "";
            ret.DownloadUrlHttps = "";
            ret.Container = "N/A";
            ret.BlobReference = "N/A";

            return ret;
        }
    }
}
