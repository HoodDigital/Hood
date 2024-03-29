﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class MediaManager : IMediaManager
    {
        private MediaSettings _mediaSettings;
        private string _container;
        private string _key;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public MediaManager(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        private async Task<BlobContainerClient> GetClientAsync()
        {
            if (_mediaSettings == null)
            {
                DbContextOptionsBuilder<HoodDbContext> options = new();
                options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
                HoodDbContext db = new(options.Options);
                Option option = db.Options.SingleOrDefault(o => o.Id == typeof(MediaSettings).ToString());
                _mediaSettings = JsonConvert.DeserializeObject<MediaSettings>(option.Value);
            }

            _container = _mediaSettings.ContainerName.ToSeoUrl();
            if (!_container.IsSet())
            {
                throw new Exception("Storage account is not set up, please go to your administration panel, and visit Settings > Media Settings, and ensure you have set a storage connection string.");
            }

            _key = _mediaSettings.AzureKey;
            if (!_key.IsSet())
            {
                throw new Exception("Storage account is not set up, please go to your administration panel, and visit Settings > Media Settings, and ensure you have set a valid container name.");
            }

            BlobContainerClient blobContainerClient = new(_key, _container);

            if (!await blobContainerClient.ExistsAsync())
            {
                await blobContainerClient.CreateIfNotExistsAsync();
                await blobContainerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);
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
            int counter = 1;
            while (await Exists(directory, filename))
            {
                filename = Guid.NewGuid().ToString() + Path.GetExtension(filename);
                counter++;
            }
            return filename;
        }

        public string GetCleanFilename(string directory, string filename)
        {
            filename = filename.Trim(Path.GetInvalidFileNameChars());
            filename = filename.Trim(Path.GetInvalidPathChars());
            filename = filename.Replace("%", "");
            filename = Path.GetFileNameWithoutExtension(filename).ToAzureFilename() + Path.GetExtension(filename);
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
            bool result = Uri.TryCreate(blobReference, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
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
                BlobReference = GetBlobReference(directoryPath, await GetSafeFilename(directoryPath, file.GetFilename())),
                UniqueId = Guid.NewGuid().ToString(),
                CreatedOn = DateTime.UtcNow,
                FileSize = file.Length,
                FileType = file.ContentType,
                Path = directoryPath
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
        public async Task<IMediaObject> ProcessUpload(Stream file, string filename, string filetype, long size, string directoryPath)
        {
            IMediaObject media = new MediaObject
            {
                Filename = filename,
                BlobReference = GetBlobReference(directoryPath, await GetSafeFilename(directoryPath, filename)),
                UniqueId = Guid.NewGuid().ToString(),
                Path = directoryPath
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
            using (var fs = new System.IO.MemoryStream())
            {
                await response.Content.CopyToAsync(fs);
                media.ThumbUrl = await GenerateThumb(media, fs, ".xs", 250);
                media.SmallUrl = await GenerateThumb(media, fs, ".sm", 600);
                media.MediumUrl = await GenerateThumb(media, fs, ".md", 1280);
                media.LargeUrl = await GenerateThumb(media, fs, ".xl", 1920);
            }
            return media;
        }

        private async Task<string> GenerateThumb(IMediaObject media, Stream stream, string prefix, int size)
        {
            try
            {
                string url = "";

                string fileName = Path.GetFileNameWithoutExtension(media.Url);
                string fileExt = Path.GetExtension(media.Url);
                string thumbBlobReference = $"{media.Path}/{fileName}{prefix}{fileExt}";
                IImageFormat format;
                stream.Position = 0;
                using (Image image = Image.Load(stream, out format))
                {
                    image.Mutate(x => x.Resize(size, 0));
                    using (Stream outputStream = new System.IO.MemoryStream())
                    {
                        image.Save(outputStream, format);
                        outputStream.Position = 0;
                        var blob = await Upload(outputStream, thumbBlobReference);
                        url = blob.Uri.ToUrlString();
                    }
                }

                return url;
            }
            catch (Exception)
            {
                // Thumbnailing failed, just send back the Url.
                return media.Url;
            }
        }

        public async Task DeleteStoredMedia(IMediaObject media)
        {
            if (media != null)
            {
                try { await Delete(media.BlobReference); } catch (Exception) { }
                try { await Remove(media.SmallUrl); } catch (Exception) { }
                try { await Remove(media.MediumUrl); } catch (Exception) { }
                try { await Remove(media.LargeUrl); } catch (Exception) { }
                try { await Remove(media.ThumbUrl); } catch (Exception) { }
            }
        }

        public const string UserDirectorySlug = "users";
        public const string SiteDirectorySlug = "site";
        public const string ContentDirectorySlug = "content";
        public const string PropertyDirectorySlug = "property";
    }
}
