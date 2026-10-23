using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SkiaSharp;

namespace Hood.Services
{
    public class MediaManager : IMediaManager
    {
        private MediaSettings _mediaSettings;
        private string _container;
        private string _key;
        private readonly IConfiguration _config;

        public MediaManager(IConfiguration config)
        {
            _config = config;
        }

        // Media storage is "configured" once a storage connection string (AzureKey) is set — ContainerName
        // defaults to a generated value so it is never the deciding factor. Used to short-circuit uploads
        // with a setup prompt instead of throwing when storage hasn't been wired up yet.
        public bool IsConfigured => Engine.Settings?.Media?.AzureKey.IsSet() == true;

        private async Task<BlobContainerClient> GetClientAsync()
        {
            if (_mediaSettings == null)
            {
                DbContextOptionsBuilder<HoodDbContext> options = new();
                options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
                HoodDbContext db = new(options.Options);
                Option option = db.Options.SingleOrDefault(o =>
                    o.Id == typeof(MediaSettings).ToString()
                );
                _mediaSettings = JsonConvert.DeserializeObject<MediaSettings>(option.Value);
            }

            _container = _mediaSettings.ContainerName.ToSeoUrl();
            if (!_container.IsSet())
            {
                throw new Exception(
                    "Storage account is not set up, please go to your administration panel, and visit Settings > Media Settings, and ensure you have set a storage connection string."
                );
            }

            _key = _mediaSettings.AzureKey;
            if (!_key.IsSet())
            {
                throw new Exception(
                    "Storage account is not set up, please go to your administration panel, and visit Settings > Media Settings, and ensure you have set a valid container name."
                );
            }

            BlobContainerClient blobContainerClient = new(_key, _container);

            if (!await blobContainerClient.ExistsAsync())
            {
                await blobContainerClient.CreateIfNotExistsAsync();
                await blobContainerClient.SetAccessPolicyAsync(
                    Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer
                );
            }

            return blobContainerClient;
        }

        #region "Helpers"
        public string GetBlobReference(string directory, string filename)
        {
            if (!directory.EndsWith("/"))
            {
                directory += "/";
            }

            if (filename.EndsWith("/"))
            {
                filename = filename.TrimEnd('/');
            }

            return string.Concat(directory, filename).ToLowerInvariant();
        }
        #endregion

        public async Task<string> GetSafeFilename(string directory, string filename)
        {
            filename = Guid.NewGuid().ToString() + Path.GetExtension(filename);
            while (await Exists(directory, filename))
            {
                filename = Guid.NewGuid().ToString() + Path.GetExtension(filename);
            }
            return filename;
        }

        public string GetCleanFilename(string filename)
        {
            filename = filename.Trim(Path.GetInvalidFileNameChars());
            filename = filename.Trim(Path.GetInvalidPathChars());
            filename = filename.Replace("%", "");
            filename =
                Path.GetFileNameWithoutExtension(filename).ToAzureFilename()
                + Path.GetExtension(filename);
            return filename;
        }

        public async Task<bool> Exists(string blobReference)
        {
            BlobContainerClient client = await GetClientAsync();
            BlockBlobClient blockBlob = client.GetBlockBlobClient(blobReference);
            try
            {
                return await blockBlob.ExistsAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> Exists(string directory, string filename)
        {
            return await Exists(GetBlobReference(directory, filename));
        }

        public async Task<bool> Delete(string blobReference)
        {
            BlobContainerClient client = await GetClientAsync();
            BlockBlobClient blockBlob = client.GetBlockBlobClient(blobReference);
            return await blockBlob.DeleteIfExistsAsync();
        }

        public async Task<bool> Delete(string directory, string filename)
        {
            return await Delete(GetBlobReference(directory, filename));
        }

        public async Task<bool> Remove(string blobReference)
        {
            // if there is an old file
            if (string.IsNullOrEmpty(blobReference))
            {
                throw new ArgumentNullException(nameof(blobReference));
            }

            // check if it is a full url - if so strip the bollocks.
            bool result =
                Uri.TryCreate(blobReference, UriKind.Absolute, out Uri uriResult)
                && (
                    uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps
                );
            if (result)
            {
                blobReference = uriResult.PathAndQuery.Replace("/" + _container, "").TrimStart('/');
            }

            // the old file exists - REMOVE IT!
            return await Delete(blobReference);
        }

        public async Task<BlockBlobClient> GetBlob(string blobReference)
        {
            BlobContainerClient client = await GetClientAsync();
            BlockBlobClient blockBlob = client.GetBlockBlobClient(blobReference);
            if (await blockBlob.ExistsAsync())
            {
                return blockBlob;
            }
            return null;
        }

        public async Task<BlockBlobClient> GetBlob(string directory, string filename)
        {
            return await GetBlob(GetBlobReference(directory, filename));
        }

        public async Task<BlockBlobClient> Upload(Stream stream, string blobReference)
        {
            BlobContainerClient client = await GetClientAsync();
            BlockBlobClient blockBlob = client.GetBlockBlobClient(blobReference);
            await blockBlob.UploadAsync(stream);
            return blockBlob;
        }

        public async Task<IMediaObject> ProcessUpload(IFormFile file, string directoryPath)
        {
            IMediaObject media = new MediaObject
            {
                Filename = file.GetFilename(),
                BlobReference = GetBlobReference(
                    directoryPath,
                    await GetSafeFilename(directoryPath, file.GetFilename())
                ),
                UniqueId = Guid.NewGuid().ToString(),
                CreatedOn = DateTime.UtcNow,
                FileSize = file.Length,
                FileType = file.ContentType,
                Path = directoryPath,
            };
            media.GenericFileType = media.FileType.ToFileType();

            // Upload the media, filename as the name, check it doesn't already exist first.
            BlockBlobClient uploadResult = await Upload(file.OpenReadStream(), media.BlobReference);

            // Attach to the entity
            media.Url = uploadResult.Uri.AbsoluteUri;

            // Process type, size etc.

            switch (media.GenericFileType)
            {
                case GenericFileType.Image:
                    media = await ProcessImage(media);
                    break;
            }

            return media;
        }

        public async Task<IMediaObject> ProcessUpload(
            Stream file,
            string filename,
            string filetype,
            string directoryPath
        )
        {
            IMediaObject media = new MediaObject
            {
                Filename = filename,
                BlobReference = GetBlobReference(
                    directoryPath,
                    await GetSafeFilename(directoryPath, filename)
                ),
                UniqueId = Guid.NewGuid().ToString(),
                Path = directoryPath,
            };

            // Upload the media, filename as the name, check it doesn't already exist first.
            BlockBlobClient uploadResult = await Upload(file, media.BlobReference);

            // Attach to the entity
            media.Url = uploadResult.Uri.AbsoluteUri;

            // Process type, size etc.
            media.CreatedOn = DateTime.UtcNow;
            media.FileSize = file.Length;
            media.FileType = filetype;
            media.GenericFileType = filetype.ToFileType();

            GenericFileType type = filetype.ToFileType();
            switch (type)
            {
                case GenericFileType.Image:
                    media = await ProcessImage(media);
                    break;
            }

            return media;
        }

        private async Task<IMediaObject> ProcessImage(IMediaObject media)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(media.Url);
            using var fs = new MemoryStream();
            await response.Content.CopyToAsync(fs);

            SKBitmap source = null;
            var format = SKEncodedImageFormat.Png;
            try
            {
                using var data = SKData.CreateCopy(fs.ToArray());
                using var codec = SKCodec.Create(data);
                if (codec != null)
                {
                    format = codec.EncodedFormat;
                    using var decoded = SKBitmap.Decode(codec);
                    source = Orient(decoded, codec.EncodedOrigin);
                }
            }
            catch (Exception ex)
            {
                await Engine.Logs.AddExceptionAsync<MediaManager>(
                    "Failed to read an uploaded image, using the original in place of thumbnails.",
                    ex,
                    LogType.Warning
                );
            }

            using (source)
            {
                media.ThumbUrl = await GenerateThumb(media, source, format, ".xs", 250);
                media.SmallUrl = await GenerateThumb(media, source, format, ".sm", 600);
                media.MediumUrl = await GenerateThumb(media, source, format, ".md", 1280);
                media.LargeUrl = await GenerateThumb(media, source, format, ".xl", 1920);
            }
            return media;
        }

        // Skia ignores the EXIF orientation tag, and its re-encoded thumbnails carry none.
        private static SKBitmap Orient(SKBitmap bitmap, SKEncodedOrigin origin)
        {
            if (bitmap == null || origin == SKEncodedOrigin.TopLeft)
            {
                return bitmap?.Copy();
            }

            float w = bitmap.Width;
            float h = bitmap.Height;
            var matrix = origin switch
            {
                SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),
                SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1),
                SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),
                SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
                SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),
                SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1),
                SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),
                _ => SKMatrix.Identity,
            };
            bool swap = origin >= SKEncodedOrigin.LeftTop;
            var oriented = new SKBitmap(
                bitmap.Info.WithSize(
                    swap ? bitmap.Height : bitmap.Width,
                    swap ? bitmap.Width : bitmap.Height
                )
            );
            using var canvas = new SKCanvas(oriented);
            canvas.SetMatrix(matrix);
            using var image = SKImage.FromBitmap(bitmap);
            canvas.DrawImage(image, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
            return oriented;
        }

        private async Task<string> GenerateThumb(
            IMediaObject media,
            SKBitmap source,
            SKEncodedImageFormat format,
            string prefix,
            int size
        )
        {
            if (source == null)
            {
                return media.Url;
            }

            try
            {
                string fileName = Path.GetFileNameWithoutExtension(media.Url);
                int height = Math.Max(
                    1,
                    (int)Math.Round((double)source.Height * size / source.Width)
                );
                using var resized = source.Resize(
                    source.Info.WithSize(size, height),
                    new SKSamplingOptions(SKCubicResampler.Mitchell)
                );
                if (resized == null)
                {
                    return media.Url;
                }

                // Skia only encodes JPEG, PNG and WebP, so every other decoded format becomes a PNG.
                var encodeFormat = format
                    is SKEncodedImageFormat.Jpeg
                        or SKEncodedImageFormat.Png
                        or SKEncodedImageFormat.Webp
                    ? format
                    : SKEncodedImageFormat.Png;
                string fileExt = encodeFormat == format ? Path.GetExtension(media.Url) : ".png";
                string thumbBlobReference = $"{media.Path}/{fileName}{prefix}{fileExt}";
                using var data = resized.Encode(encodeFormat, 90);
                if (data == null)
                {
                    return media.Url;
                }

                using Stream outputStream = data.AsStream();
                var blob = await Upload(outputStream, thumbBlobReference);
                return blob.Uri.ToUrlString();
            }
            catch (Exception ex)
            {
                await Engine.Logs.AddExceptionAsync<MediaManager>(
                    "Failed to generate a thumbnail, using the original.",
                    ex,
                    LogType.Warning
                );
                return media.Url;
            }
        }

        public async Task DeleteStoredMedia(IMediaObject media)
        {
            if (media != null)
            {
                try
                {
                    await Delete(media.BlobReference);
                }
                catch (Exception ex)
                {
                    await Engine.Logs.AddExceptionAsync<MediaManager>(
                        "Failed to delete stored media file.",
                        ex,
                        LogType.Warning
                    );
                }
                try
                {
                    await Remove(media.SmallUrl);
                }
                catch (Exception ex)
                {
                    await Engine.Logs.AddExceptionAsync<MediaManager>(
                        "Failed to delete stored media file.",
                        ex,
                        LogType.Warning
                    );
                }
                try
                {
                    await Remove(media.MediumUrl);
                }
                catch (Exception ex)
                {
                    await Engine.Logs.AddExceptionAsync<MediaManager>(
                        "Failed to delete stored media file.",
                        ex,
                        LogType.Warning
                    );
                }
                try
                {
                    await Remove(media.LargeUrl);
                }
                catch (Exception ex)
                {
                    await Engine.Logs.AddExceptionAsync<MediaManager>(
                        "Failed to delete stored media file.",
                        ex,
                        LogType.Warning
                    );
                }
                try
                {
                    await Remove(media.ThumbUrl);
                }
                catch (Exception ex)
                {
                    await Engine.Logs.AddExceptionAsync<MediaManager>(
                        "Failed to delete stored media file.",
                        ex,
                        LogType.Warning
                    );
                }
            }
        }

        public const string UserDirectorySlug = "users";
        public const string SiteDirectorySlug = "site";
        public const string ContentDirectorySlug = "content";
        public const string PropertyDirectorySlug = "property";
    }
}
