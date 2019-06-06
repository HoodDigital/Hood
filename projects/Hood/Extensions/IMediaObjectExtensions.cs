using Hood.Core;
using Hood.Enums;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using System;

namespace Hood.Extensions
{
    public static class IMediaObjectExtensions
    {
        public static IMediaObject UpdateUrls(this IMediaObject target, IMediaObject source)
        {
            target.LargeUrl = source.LargeUrl;
            target.SmallUrl = source.SmallUrl;
            target.MediumUrl = source.MediumUrl;
            target.ThumbUrl = source.ThumbUrl;
            target.Url = source.Url;
            target.UniqueId = source.UniqueId;
            return target;
        }
        public static IMediaObject UpdateHostName(this IMediaObject mediaObject, string hostname)
        {
            mediaObject.LargeUrl = new Uri(mediaObject.LargeUrl).ToUrlString(hostname);
            mediaObject.SmallUrl = new Uri(mediaObject.SmallUrl).ToUrlString(hostname);
            mediaObject.MediumUrl = new Uri(mediaObject.MediumUrl).ToUrlString(hostname);
            mediaObject.ThumbUrl = new Uri(mediaObject.ThumbUrl).ToUrlString(hostname);
            mediaObject.Url = new Uri(mediaObject.Url).ToUrlString(hostname);
            return mediaObject;
        }
        public static string ToIcon(this IMediaObject mediaObject)
        {
            GenericFileType type = GenericFileType.Unknown;
            string output = "";
            if (Enum.TryParse(mediaObject.GeneralFileType, out type))
            {
                switch (type)
                {
                    case GenericFileType.Image:
                        output = mediaObject.SmallUrl;
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
                    case GenericFileType.Audio:
                        return "/lib/hood/images/icons/audio.png";
                    case GenericFileType.Video:
                        return "/lib/hood/images/icons/video.png";
                    case GenericFileType.Unknown:
                    default:
                        return "/lib/hood/images/icons/file.png";
                }
            }
            if (!output.IsSet())
            {
                output = MediaObjectBase.NoImageUrl;
            }
            return output;
        }
        public static string ToJson(this IMediaObject mediaObject)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(mediaObject);
        }

    }

}
