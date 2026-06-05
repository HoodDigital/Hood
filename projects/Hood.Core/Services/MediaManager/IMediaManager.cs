using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Specialized;
using Hood.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Hood.Services
{
    public interface IMediaManager
    {
        /// <summary>
        /// True when media storage is configured (a storage connection string has been set in
        /// Settings &gt; Media Settings). When false, uploads cannot proceed and callers should
        /// surface a setup prompt rather than attempting storage operations.
        /// </summary>
        bool IsConfigured { get; }

        string GetBlobReference(string directory, string filename);

        /// <summary>
        /// Gets a safe filename for use with Azure storage. Unsafe characters will be removed.
        /// If the filename is already present, a number will be postfixed.
        /// </summary>
        /// <param name="filename">The filename. The clean filename will be calculated from this.</param>
        /// <returns></returns>
        string GetCleanFilename(string filename);
        Task<string> GetSafeFilename(string path, string filename);

        /// <summary>
        /// Uploads the specified IFormFile object to the Azure storage, at the location specified in the path parameter.
        /// </summary>
        /// <param name="file">The file stream to upload.</param>
        /// <param name="blobReference">The blob reference (path) on Azure storage.</param>
        /// <returns></returns>
        Task<BlockBlobClient> Upload(Stream file, string blobReference);

        /// <summary>
        /// Check if the blob exists currently on Azure storage.
        /// </summary>
        /// <param name="blobReference">The blobReference of the item to check.</param>
        /// <returns></returns>
        Task<bool> Exists(string blobReference);

        /// <summary>
        /// Check if the file exists currently on Azure storage.
        /// </summary>
        /// <param name="path">Path to the file on Azure storage.</param>
        /// <param name="filename">The filename to check.</param>
        /// <returns></returns>
        Task<bool> Exists(string path, string filename);

        /// <summary>
        /// Delete the blob currently on Azure storage.
        /// </summary>
        /// <param name="blobReference">The blobReference of the item to check.</param>
        /// <returns></returns>
        Task<bool> Delete(string blobReference);

        /// <summary>
        /// Delete the file currently on Azure storage.
        /// </summary>
        /// <param name="path">Path to the file on Azure storage.</param>
        /// <param name="filename">The filename to check.</param>
        /// <returns></returns>
        Task<bool> Delete(string path, string filename);

        /// <summary>
        /// Delete the file currently on Azure storage. This can be done with a Url or a blobReference.
        /// </summary>
        /// <param name="blobReference">Blob Reference or Url to the file on Azure storage.</param>
        /// <returns></returns>
        Task<bool> Remove(string blobReference);

        /// <summary>
        /// Complete function to take an Http uploaded file, check it's contents, add it's information to an IMediaItem object, if the file is an image,
        /// thumbnails will be processed, uploaded to Azure storage and placed into the IMediaItem.
        /// </summary>
        /// <param name="file">IFormFile object (Http File Request object)</param>
        /// <param name="directoryPath">The directory path on storage to upload into.</param>
        /// <returns></returns>
        Task<IMediaObject> ProcessUpload(IFormFile file, string directoryPath);

        /// <summary>
        /// Complete function to take an file stream and basic file data, check it's contents, add it's information to an IMediaItem object, if the file is an image,
        /// thumbnails will be processed, uploaded to Azure storage and placed into the IMediaItem.
        /// </summary>
        /// <param name="file">The file stream to upload.</param>
        /// <param name="filename">The original filename.</param>
        /// <param name="filetype">The content (MIME) type of the file.</param>
        /// <param name="directoryPath">The directory path on storage to upload into.</param>
        /// <returns></returns>
        Task<IMediaObject> ProcessUpload(
            Stream file,
            string filename,
            string filetype,
            string directoryPath
        );

        Task<BlockBlobClient> GetBlob(string blobReference);

        Task<BlockBlobClient> GetBlob(string directory, string filename);

        /// <summary>
        /// Deletes any stored files associated with the TMediaItem object.
        /// </summary>
        /// <param name="media">The media item to remove all associated media from Azure.</param>
        /// <returns></returns>
        Task DeleteStoredMedia(IMediaObject media);
    }
}
