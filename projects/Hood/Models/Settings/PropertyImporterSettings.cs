using Hood.Enums;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class PropertyImporterSettings
    {
        /// <summary>
        /// Whether or not to use the FTP Service to download the BLM & Image files when using BLM Importer.
        /// </summary>
        [Display(Name = "Enable the property importer", Description = "Enable the property importer service on this website.")]
        public bool Enabled { get; set; }
        /// <summary>
        /// This is the type of importer to use, can be BLM File or Web API.
        /// </summary>
        [Display(Name = "Import Method", Description = "What service are you importing your property files from?")]
        public PropertyImporterMethod Method { get; set; }

        /// <summary>
        /// Whether or not to use the FTP Service to download the BLM & Image files when using BLM Importer.
        /// </summary>
        [Display(Name = "Clear images on import", Description = "Clear all images from each property before importing new ones. Leave this unchecked to add/update existing images.")]
        public bool ClearImagesBeforeImport { get; set; }
        
        /// <summary>
        /// The FTP Server address that is used for the FTP Import
        /// </summary>
        [Display(Name = "Remote FTP server address", Description = "The remote server to connect to to download BLM and image/media files e.g. <code>ftp.yourdomain.com</code>")]
        public string Server { get; set; }

        /// <summary>
        /// The BLM Filename to be used with the BLM File importer
        /// </summary>
        [Display(Name = "BLM filename", Description = "The filename of your BLM file, include the extension, but not the location e.g. <code>properties.blm</code>")]
        public string Filename { get; set; }

        /// <summary>
        /// This is the local folder where files are loaded from if FTP is not used.
        /// </summary>
        [Display(Name = "Local Folder", Description = "Local folder to load files from. This must be a directory located in your site's wwwroot folder.")]
        public string LocalFolder { get; set; }

        /// <summary>
        /// Username for accessing FTP Services or Web APIs
        /// </summary>
        [Display(Name = "Username", Description = "Password for the remote FTP that you are connecting to.")]
        public string Username { get; set; }

        /// <summary>
        /// Password for accessing FTP Services or Web APIs
        /// </summary>
        [Display(Name = "Password", Description = "Password for the remote FTP that you are connecting to.")]
        public string Password { get; set; }

        /// <summary>
        /// Does the service require a file unzip before importing?
        /// </summary>
        [Display(Name = "Require Unzip", Description = "Does the file require unzipping before the BLM import?")]
        public bool RequireUnzip { get; set; }

        /// <summary>
        /// Name of the zip file which contains the import data
        /// </summary>
        [Display(Name = "Unzip File name", Description = "Name of the Zip file to extract before BLM import.")]
        public string ZipFile { get; set; }

        /// <summary>
        /// Name of the zip file which contains the import data
        /// </summary>
        [Display(Name = "Removal of Extraneous Properties", Description = "How do you want extraneous properties to be handled by the property importer.")]
        public ExtraneousPropertyProcess ExtraneousPropertyProcess { get; set; }
    }

}

