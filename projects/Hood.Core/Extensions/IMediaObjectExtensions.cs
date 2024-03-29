﻿using Hood.Core;
using Hood.Enums;
using Hood.Interfaces;
using Hood.Models;
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
            string output;
            switch (mediaObject.GenericFileType)
            {
                case GenericFileType.Image:
                    output = mediaObject.SmallUrl;
                    break;
                case GenericFileType.Excel:
                    return Engine.Resource("/images/icons/excel.png");
                case GenericFileType.PDF:
                    return Engine.Resource("/images/icons/pdf.png");
                case GenericFileType.PowerPoint:
                    return Engine.Resource("/images/icons/powerpoint.png");
                case GenericFileType.Word:
                    return Engine.Resource("/images/icons/word.png");
                case GenericFileType.Photoshop:
                    return Engine.Resource("/images/icons/photoshop.png");
                case GenericFileType.Audio:
                    return Engine.Resource("/images/icons/audio.png");
                case GenericFileType.Video:
                    return Engine.Resource("/images/icons/video.png");
                case GenericFileType.Unknown:
                default:
                    return Engine.Resource("/images/icons/file.png");
            }
            if (!output.IsSet())
            {
                output = MediaBase.NoImageUrl;
            }
            return output;
        }
        public static string ToJson(this IMediaObject mediaObject)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(mediaObject);
        }
    }

}
