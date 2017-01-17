using Hood.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IMediaManager<TMediaObject>
        where TMediaObject : IMediaObject
    {
        string GetBlobReference(string directory, string filename);
        string GetSeoDirectory(string directory); 

        /// <summary>
        /// Gets a safe filename for use with Azure storage. Unsafe characters will be removed.
        /// If the filename is already present, a number will be postfixed.
        /// </summary>
        /// <param name="file">The filename. Filename will be calclulated from this.</param>
        /// <param name="directory">The base path for the destination on Azure storage. In the form {0}/{1}/</param>
        /// <returns></returns>
        string GetCleanFilename(string path, string filename);
        Task<string> GetSafeFilename(string path, string filename);

        /// <summary>
        /// Uploads the specified IFormFile object to the Azure storage, at the location specified in the path parameter.
        /// </summary>
        /// <param name="file">IFormFile object (Http File Request object)</param>
        /// <param name="path">Path on Azure storage.</param>
        /// <returns></returns>
        Task<CloudBlockBlob> Upload(Stream file, string blobReference);

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
        /// <param name="media">Object of type IMediaItem to contain the file information and details aqcuired in the upload process.</param>
        /// <returns></returns>
        Task<TMediaObject> ProcessUpload(IFormFile file, TMediaObject media);
        /// <summary>
        /// Complete function to take an file stream and basic file data, check it's contents, add it's information to an IMediaItem object, if the file is an image, 
        /// thumbnails will be processed, uploaded to Azure storage and placed into the IMediaItem.
        /// </summary>
        /// <param name="media">Object of type IMediaItem to contain the file information and details aqcuired in the upload process.</param>
        /// <returns></returns>
        Task<TMediaObject> ProcessUpload(Stream file, string filename, string filetype, long size, TMediaObject media);

        Task<string> UploadToSharedAccess(Stream file, string filename, DateTimeOffset? expiry, SharedAccessBlobPermissions permissions = SharedAccessBlobPermissions.Read);

        /// <summary>
        /// Deletes any stored files associated with the TMediaItem object.
        /// </summary>
        /// <param name="media">The media item to remove all associated media from Azure.</param>
        /// <returns></returns>
        Task DeleteStoredMedia(TMediaObject media);
    }
}
