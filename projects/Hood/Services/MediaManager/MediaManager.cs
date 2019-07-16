using Hood.Caching;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class MediaManager : IMediaManager
    {
        private CloudStorageAccount _storageAccount;
        private string _container;
        private string _key;
        private readonly IHostingEnvironment _env;

        public MediaManager(IHostingEnvironment env)
        {
            _env = env;
            Initialise();
        }

        private void CheckStorage()
        {
            Initialise();
            if (_storageAccount == null)
            {
                throw new Exception("Storage account is not set up, please go to your administration panel, and visit Settings > Media Settings, and add a valid Azure Storage Key.");
            }
        }

        private void Initialise()
        {
            MediaSettings _mediaSettings = Engine.Settings.Media;
            _container = _mediaSettings.ContainerName.ToSeoUrl();
            _key = _mediaSettings.AzureKey;
            try
            {
                _storageAccount = CloudStorageAccount.Parse(_key);
            }
            catch (Exception)
            {
                _storageAccount = null;
            }
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
            CheckStorage();
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(_container);
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobReference);
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
            CheckStorage();
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(_container);
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobReference);
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
                throw new ArgumentNullException("blobReference");
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

        public async Task<CloudBlockBlob> GetBlob(string blobReference)
        {
            CheckStorage();
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(_container);
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobReference);
            if (await blockBlob.ExistsAsync())
            {
                return blockBlob;
            }
            return null;
        }
        public async Task<CloudBlockBlob> GetBlob(string directory, string filename)
        {
            return await GetBlob(GetBlobReference(directory, filename));
        }

        public async Task<CloudBlockBlob> Upload(Stream stream, string blobReference)
        {
            CheckStorage();
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(_container);

            // If container doesn’t exist, create it.
            await blobContainer.CreateIfNotExistsAsync();
            await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });

            // Get a reference to the blob named blobReference
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobReference);
            await blockBlob.UploadFromStreamAsync(stream);
            return blockBlob;
        }

        public async Task<IMediaObject> ProcessUpload(IFormFile file, string directoryPath)
        {
            IMediaObject media = new MediaObject
            {
                Filename = file.GetFilename(),
                BlobReference = GetBlobReference(directoryPath, await GetSafeFilename(directoryPath, file.GetFilename())),
                UniqueId = Guid.NewGuid().ToString(),
                CreatedOn = DateTime.Now,
                FileSize = file.Length,
                FileType = file.ContentType,
                Path = directoryPath
            };
            media.GenericFileType = media.FileType.ToFileType();

            // Upload the media, filename as the name, check it doesn't already exist first.
            CloudBlockBlob uploadResult = await Upload(file.OpenReadStream(), media.BlobReference);

            // Attach to the entity
            media.Url = uploadResult.StorageUri.PrimaryUri.ToUrlString();

            // Process type, size etc.

            switch (media.GenericFileType)
            {
                case GenericFileType.Image:
                    media = await ProcessImageAsync(media);
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
            CloudBlockBlob uploadResult = await Upload(file, media.BlobReference);

            // Attach to the entity
            media.Url = uploadResult.StorageUri.PrimaryUri.ToUrlString();

            // Process type, size etc.
            media.CreatedOn = DateTime.Now;
            media.FileSize = file.Length;
            media.FileType = filetype;
            media.GenericFileType = filetype.ToFileType();

            GenericFileType type = filetype.ToFileType();
            switch (type)
            {
                case GenericFileType.Image:
                    media = await ProcessImageAsync(media);
                    break;
            }

            return media;
        }

        public async Task<string> UploadToSharedAccess(Stream file, string filename, DateTimeOffset? expiry, SharedAccessBlobPermissions permissions = SharedAccessBlobPermissions.Read)
        {
            string sasBlobToken;

            // Upload the media, filename as the name, check it doesn't already exist first.
            CheckStorage();
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(_container + "-secure");

            // If container doesn’t exist, create it.
            await blobContainer.CreateIfNotExistsAsync();
            await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Off
            });

            // Get a reference to the blob named blobReference
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);
            await blockBlob.UploadFromStreamAsync(file);

            // Create a new access policy and define its constraints.
            // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad-hoc SAS, and
            // to construct a shared access policy that is saved to the container's shared access policies.
            SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
            {
                // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                SharedAccessExpiryTime = expiry ?? DateTime.UtcNow.AddHours(24),
                Permissions = permissions
            };

            // Generate the shared access signature on the blob, setting the constraints directly on the signature.
            sasBlobToken = blockBlob.GetSharedAccessSignature(adHocSAS);

            Console.WriteLine("SAS for blob (ad hoc): {0}", sasBlobToken);
            Console.WriteLine();

            return blockBlob.Uri + sasBlobToken;
        }

        private async Task<IMediaObject> ProcessImageAsync(IMediaObject media)
        {
            string fileName = Path.GetFileNameWithoutExtension(media.Url);
            string fileExt = Path.GetExtension(media.Url);
            string tempDir = _env.ContentRootPath + "\\Temporary\\" + typeof(ImageProcessor) + "\\";
            string tempGuid = Guid.NewGuid().ToString();
            string tempFileName = tempDir + tempGuid + fileExt;

            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            // download the file.
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(media.Url, tempFileName);
            }

            // create three thumbnailed versions, and add to the array of files to upload.
            string XsFilename = string.Format("{0}/{1}_xs{2}", media.Path, fileName, fileExt);
            string SmFilename = string.Format("{0}/{1}_sm{2}", media.Path, fileName, fileExt);
            string MdFilename = string.Format("{0}/{1}_md{2}", media.Path, fileName, fileExt);
            string LgFilename = string.Format("{0}/{1}_lg{2}", media.Path, fileName, fileExt);
            string tempXs = tempDir + tempGuid + "_xs" + fileExt;
            string tempSm = tempDir + tempGuid + "_sm" + fileExt;
            string tempMd = tempDir + tempGuid + "_md" + fileExt;
            string tempLg = tempDir + tempGuid + "_lg" + fileExt;

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

            ImageProcessor.ResizeImage(tempFileName, tempXs, 250, 250, format);
            ImageProcessor.ResizeImage(tempFileName, tempSm, 640, 640, format);
            ImageProcessor.ResizeImage(tempFileName, tempMd, 1280, 1280, format);
            ImageProcessor.ResizeImage(tempFileName, tempLg, 1920, 1920, format);

            // foreach, file in the list of thumbnails
            // upload to the thumbLocation
            using (FileStream fs = File.OpenRead(tempXs)) { media.ThumbUrl = (await Upload(fs, XsFilename)).Uri.ToUrlString(); }
            using (FileStream fs = File.OpenRead(tempSm)) { media.SmallUrl = (await Upload(fs, SmFilename)).Uri.ToUrlString(); }
            using (FileStream fs = File.OpenRead(tempMd)) { media.MediumUrl = (await Upload(fs, MdFilename)).Uri.ToUrlString(); }
            using (FileStream fs = File.OpenRead(tempLg)) { media.LargeUrl = (await Upload(fs, LgFilename)).Uri.ToUrlString(); }

            // add the url to the array of urls to send back
            // remove the temporary image
            try { File.Delete(tempFileName); } catch (Exception) { }
            try { File.Delete(tempXs); } catch (Exception) { }
            try { File.Delete(tempSm); } catch (Exception) { }
            try { File.Delete(tempMd); } catch (Exception) { }
            try { File.Delete(tempLg); } catch (Exception) { }

            return media;
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
            MediaObject old = new MediaObject();
            media.CopyProperties(old);

            // download the orignal file, and save it to temp.
            string tempFile = tempDirectory + media.UniqueId;
            using (WebClient client = new WebClient())
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
    }
}
