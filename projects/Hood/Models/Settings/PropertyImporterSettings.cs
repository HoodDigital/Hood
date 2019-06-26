using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class PropertyImporterSettings
    {
        /// <summary>
        /// This is the type of importer to use, can be BLM File or Web API.
        /// </summary>
        public PropertyImporterMethod Method { get; set; }

        /// <summary>
        /// Whether or not to use the FTP Service to download the BLM & Image files when using BLM Importer.
        /// </summary>
        [Display(Name = "Download from Remote FTP Server")]
        public bool UseFTP { get; set; }
        /// <summary>
        /// Whether or not to use the FTP Service to download the BLM & Image files when using BLM Importer.
        /// </summary>
        [Display(Name = "Clear Images Before Downloading new Images")]
        public bool ClearImagesBeforeImport { get; set; }
        
        /// <summary>
        /// The FTP Server address that is used for the FTP Import
        /// </summary>
        [Display(Name = "Remote FTP Server Address")]
        public string Server { get; set; }

        /// <summary>
        /// The BLM Filename to be used with the BLM File importer
        /// </summary>
        [Display(Name = "BLM Filename (On FTP or Local)")]
        public string Filename { get; set; }

        /// <summary>
        /// This is the local folder where files are loaded from if FTP is not used.
        /// </summary>
        [Display(Name = "Local folder to load files from (When not using FTP)")]
        public string LocalFolder { get; set; }

        /// <summary>
        /// Username for accessing FTP Services or Web APIs
        /// </summary>
        [Display(Name = "API/FTP Username")]
        public string Username { get; set; }

        /// <summary>
        /// Password for accessing FTP Services or Web APIs
        /// </summary>
        [Display(Name = "API/FTP Password")]
        public string Password { get; set; }

        /// <summary>
        /// Does the service require a file unzip before importing?
        /// </summary>
        [Display(Name = "Require Unzip")]
        public bool RequireUnzip { get; set; }

        /// <summary>
        /// Name of the zip file which contains the import data
        /// </summary>
        [Display(Name = "Unzip File name")]
        public string ZipFile { get; set; }

    }

}

