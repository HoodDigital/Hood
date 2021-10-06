using Azure.Storage.Blobs;
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
using System;
using System.IO;
using System.Linq;
using System.Net;
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
                    media = ProcessImage(media);
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
                    media = ProcessImage(media);
                    break;
            }

            return media;
        }

        #warning Todo: Redo the Upload to shared access/ download via shared access on media service.

        //public async Task<string> UploadToSharedAccess(Stream file, string filename, DateTimeOffset? expiry, SharedAccessBlobPermissions permissions = SharedAccessBlobPermissions.Read)
        //{
        //    string sasBlobToken;

        //    // Upload the media, filename as the name, check it doesn't already exist first.
        //    var client = await GetClientAsync();
        //    var blockBlob = client.GetBlockBlobClient(blobReference);

        //    // If container doesn’t exist, create it.
        //    await blobContainer.CreateIfNotExistsAsync();
        //    await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
        //    {
        //        PublicAccess = BlobContainerPublicAccessType.Off
        //    });

        //    // Get a reference to the blob named blobReference
        //    CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);
        //    await blockBlob.UploadFromStreamAsync(file);

        //    // Create a new access policy and define its constraints.
        //    // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad-hoc SAS, and
        //    // to construct a shared access policy that is saved to the container's shared access policies.
        //    SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
        //    {
        //        // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
        //        // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
        //        SharedAccessExpiryTime = expiry ?? DateTime.UtcNow.AddHours(24),
        //        Permissions = permissions
        //    };

        //    // Generate the shared access signature on the blob, setting the constraints directly on the signature.
        //    sasBlobToken = blockBlob.GetSharedAccessSignature(adHocSAS);

        //    Console.WriteLine("SAS for blob (ad hoc): {0}", sasBlobToken);
        //    Console.WriteLine();

        //    return blockBlob.Uri + sasBlobToken;
        //}



        private IMediaObject ProcessImage(IMediaObject media)
        {
            string fileName = Path.GetFileNameWithoutExtension(media.Url);
            string fileExt = Path.GetExtension(media.Url);
            string tempDir = _env.ContentRootPath + "\\Temporary\\" + typeof(ImageProcessor) + "\\";

            string tempGuid = Guid.NewGuid().ToString();
            string tempFileName = tempDir + tempGuid + fileExt;

            // create three thumbnailed versions, and add to the array of files to upload.

            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            // download the file.
            using (WebClient client = new())
            {
                client.DownloadFile(media.Url, tempFileName);
            }

            Task[] tasks = new Task[4]
            {
                Task.Factory.StartNew(() => media.ThumbUrl = GenerateThumb(media, tempFileName, tempGuid, ".xs", 250)),
                Task.Factory.StartNew(() => media.SmallUrl = GenerateThumb(media, tempFileName, tempGuid, ".sm", 600)),
                Task.Factory.StartNew(() => media.MediumUrl = GenerateThumb(media, tempFileName, tempGuid, ".md", 1280)),
                Task.Factory.StartNew(() => media.LargeUrl = GenerateThumb(media, tempFileName, tempGuid, ".xl", 1920))
            };

            //Block until all tasks complete.
            Task.WaitAll(tasks);
            return media;
        }

        private string GenerateThumb(IMediaObject media, string tempFileName, string tempGuid, string prefix, int size)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(media.Url);
                string fileExt = Path.GetExtension(media.Url);
                string tempDir = _env.ContentRootPath + "\\Temporary\\" + typeof(ImageProcessor) + "\\";

                string tempThumbFile = tempDir + tempGuid + prefix + fileExt;
                string thumbFilename = $"{media.Path}/{fileName}{prefix}{fileExt}";

                System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Jpeg;
                switch (fileExt.ToLowerInvariant())
                {
                    case ".gif":
                        format = System.Drawing.Imaging.ImageFormat.Gif;
                        break;
                    case ".bmp":
                        format = System.Drawing.Imaging.ImageFormat.Bmp;
                        break;
                    case ".png":
                        format = System.Drawing.Imaging.ImageFormat.Png;
                        break;
                }

                ImageProcessor.ResizeImage(tempFileName, tempThumbFile, size, size, format);

                // foreach, file in the list of thumbnails
                // upload to the thumbLocation
                string url = "";
                using (FileStream fs = File.OpenRead(tempThumbFile))
                {
                    url = Upload(fs, thumbFilename).Result.Uri.ToUrlString();
                }

                // add the url to the array of urls to send back
                // remove the temporary image
                try { File.Delete(tempFileName); } catch (Exception) { }
                try { File.Delete(tempThumbFile); } catch (Exception) { }

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

        public async Task<IMediaObject> RefreshMedia(IMediaObject media, string tempDirectory)
        {
            // copy record of original files into new object
            MediaObject old = new();
            media.CopyProperties(old);

            // download the orignal file, and save it to temp.
            string tempFile = tempDirectory + media.UniqueId;
            using (WebClient client = new())
            {
                client.DownloadFile(new Uri(media.Url), tempFile);
            }

            using (FileStream fileStream = File.OpenRead(tempFile))
            {
                // reupload to a new location, and process.
                await ProcessUpload(fileStream, media.Filename, media.FileType, media.FileSize, media.Path);
            }

            // save the new file urls to the media object
            return media;
        }

        public const string UserDirectorySlug = "users";
        public const string SiteDirectorySlug = "site";
        public const string ContentDirectorySlug = "content";
        public const string PropertyDirectorySlug = "property";
    }
}
