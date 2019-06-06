using Hood.Enums;
using Hood.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Hood.Extensions
{
    public static class IFormFileExtensions
    {
        private const int Megabyte = 1048576;

        public static string GetFilename(this IFormFile file)
        {
            string filename = ContentDispositionHeaderValue
                      .Parse(file.ContentDisposition)
                      .FileName
                      .ToString()
                      .Trim('"');
            return filename;
        }

        public static GenericFileType ToFileType(this string fileType)
        {
            switch (fileType)
            {
                case "video/x-flv":
                case "application/x-mpegURL":
                case "video/MP2T":
                case "video/3gpp":
                case "video/quicktime":
                case "video/x-msvideo":
                case "video/x-ms-wmv":
                case "video/webm":
                case "application/ogg":
                case "video/ogg":
                case "video/mp4":
                    return GenericFileType.Video;
                case "audio/wav":
                case "audio/ogg":
                case "audio/mpeg":
                case "audio/mp3":
                case "audio/flac":
                case "audio/x-flac":
                case "audio/webm":
                    return GenericFileType.Audio;
                case "image/gif":
                case "image/jpeg":
                case "image/png":
                case "image/bmp":
                case "image/tiff":
                    return GenericFileType.Image;
                case "application/vnd.ms-excel":
                case "application/msexcel":
                case "application/x-msexcel":
                case "application/x-ms-excel":
                case "application/x-excel":
                case "application/x-dos_ms_excel":
                case "application/xls":
                case "application/x-xls":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return GenericFileType.Excel;
                case "application/msword":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.template":
                case "application/vnd.ms-word.document.macroEnabled.12":
                case "application/vnd.ms-word.template.macroEnabled.12":
                    return GenericFileType.Word;
                case "application/vnd.ms-powerpoint":
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                case "application/vnd.openxmlformats-officedocument.presentationml.template":
                case "application/vnd.openxmlformats-officedocument.presentationml.slideshow":
                case "application/vnd.ms-powerpoint.addin.macroEnabled.12":
                case "application/vnd.ms-powerpoint.presentation.macroEnabled.12":
                case "application/vnd.ms-powerpoint.template.macroEnabled.12":
                case "application/vnd.ms-powerpoint.slideshow.macroEnabled.12":
                    return GenericFileType.PowerPoint;
                case "application/pdf":
                case "application/x-pdf":
                    return GenericFileType.PDF;
                case "image/photoshop":
                case "image/x-photoshop":
                case "image/psd":
                case "application/photoshop":
                case "application/psd":
                case "zz-application/zz-winassoc-psd":
                    return GenericFileType.Photoshop;
                default:
                    return GenericFileType.Unknown;
            }

        }

        public static OperationResult CheckImage(this IFormFile file)
        {
            OperationResult result = new OperationResult(true);

            // Check the file, every time an error is found, flag the result, and add to the error list
            if (file.ContentType.ToFileType() != GenericFileType.Image)
                result.AddError("The provided file is not a valid image of type JPG, GIF or PNG.");

            if (file.Length > 6 * Megabyte)
                result.AddError("The provided file is over 6Mb in size, this is inadvisable for an image file.");

            return result;
        }

    }
}
