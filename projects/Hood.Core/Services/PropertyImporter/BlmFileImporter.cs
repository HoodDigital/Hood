using Geocoding.Google;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class BlmFileImporter : IPropertyImporter
    {
        private readonly IFTPService _ftp;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IDirectoryManager _directoryManager;
        private readonly IHttpContextAccessor _context;

        public BlmFileImporter(
            IFTPService ftp,
            IWebHostEnvironment env,
            IHttpContextAccessor context,
            IConfiguration config,
            IAddressService address,
            ILogService logService,
            IDirectoryManager directoryManager
            )
        {
            _ftp = ftp;
            _env = env;
            _config = config;
            _directoryManager = directoryManager;
            Lock = new ReaderWriterLock();
            Tasks = 0;
            Processed = 0;
            Updated = 0;
            Added = 0;
            Deleted = 0;
            ToAdd = 0;
            ToDelete = 0;
            ToUpdate = 0;
            Running = false;
            Cancelled = false;
            Succeeded = false;
            RemoteList = new List<string>();
            Errors = new List<string>();
            Warnings = new List<string>();
            StatusMessage = "Not running...";
            TempFolder = env.ContentRootPath + "\\Temporary\\" + typeof(BlmFileImporter) + "\\";
            _propertySettings = Engine.Settings.Property;

            LocalFolder = env.ContentRootPath + "\\" + _propertySettings.FTPImporterSettings.LocalFolder;
            _context = context;
            _address = address;
            _logService = logService;
        }

        private ReaderWriterLock Lock { get; set; }
        private bool Running { get; set; }
        private bool Succeeded { get; set; }
        private List<string> Errors { get; set; }
        private List<string> Warnings { get; set; }
        private int Tasks { get; set; }
        private int CompletedTasks { get; set; }
        private int Processed { get; set; }
        private int Updated { get; set; }
        private int Added { get; set; }
        private int ToDelete { get; set; }
        private int ToUpdate { get; set; }
        private int ToAdd { get; set; }
        private int Deleted { get; set; }
        private string StatusMessage { get; set; }
        private string TempFolder { get; set; }
        private string LocalFolder { get; set; }
        private bool Cancelled { get; set; }
        private bool FileError { get; set; }
        private List<string> RemoteList { get; set; }
        private ApplicationUser User { get; set; }
        private IMediaManager _media;
        private PropertySettings _propertySettings;
        private HoodDbContext _db { get; set; }
        private bool _killFlag;
        private readonly IAddressService _address;
        private readonly ILogService _logService;
        private string DirectoryPath { get; set; }

        public async Task RunUpdate(HttpContext context)
        {
            try
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Tasks = 0;
                CompletedTasks = 0;
                Processed = 0;
                Updated = 0;
                Added = 0;
                Deleted = 0;
                ToAdd = 0;
                ToDelete = 0;
                ToUpdate = 0;
                Running = true;
                Cancelled = false;
                Succeeded = false;
                FileError = false;
                RemoteList = new List<string>();
                Errors = new List<string>();
                Warnings = new List<string>();
                StatusMessage = "Starting import, loading property files from FTP Service...";
                _propertySettings = Engine.Settings.Property;
                // Get a new instance of the HoodDbContext for this import.
                DbContextOptionsBuilder<HoodDbContext> options = new DbContextOptionsBuilder<HoodDbContext>();
                options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
                _db = new HoodDbContext(options.Options);

                _media = new MediaManager(_env, _config);

                
                User = _db.Users.SingleOrDefault(u => u.Email == Engine.SiteOwnerEmail);
                if (User == null)
                {
                    throw new Exception("Could not load the site admin account to use for importing.");
                }

                MediaDirectory propertyDirectory = _db.MediaDirectories.SingleOrDefault(md => md.Slug == MediaManager.PropertyDirectorySlug && md.Type == DirectoryType.System);
                if (propertyDirectory == null)
                {
                    throw new Exception("Could not load the Property directory.");
                }

                DirectoryPath = _directoryManager.GetPath(propertyDirectory.Id);

                Lock.ReleaseWriterLock();

                ThreadStart pts = new ThreadStart(Import);
                Thread thread = new Thread(pts)
                {
                    Name = "Import",
                    Priority = ThreadPriority.Normal
                };
                thread.Start();
            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Running = false;
                StatusMessage = ex.Message;
                Lock.ReleaseWriterLock();
                await _logService.AddExceptionAsync<BlmFileImporter>("An error occurred starting a property update.", ex);
            }
        }

        /// <summary>
        /// This is the main thread, controls the whole update process. 
        /// All other functions are fired of from here and once complete, the update is complete.
        /// </summary>
        private async void Import()
        {
            try
            {
                // Start by cleaning the temp folder.
                CleanTempFolder();

                // open a socket to the FTP site, and pull the property file out
                switch (_propertySettings.FTPImporterSettings.Method)
                {
                    case PropertyImporterMethod.Directory:
                        if (_propertySettings.FTPImporterSettings.RequireUnzip)
                        {
                            UnzipLocalFile();
                        }
                        await GetFileFromLocalAsync(_propertySettings.FTPImporterSettings.Filename);
                        break;
                    case PropertyImporterMethod.FtpBlm:
                        GetFileFromFtp(_propertySettings.FTPImporterSettings.Filename);
                        break;
                }

                if (HasFileError())
                {
                    throw new Exception("There was a problem downloading the properties file. Please try again.");
                }

                // Go through the file, and extract key/value pairs of
                List<Dictionary<string, string>> properties = await GetPropertiesFromFileAsync();

                List<PropertyListing> feedProperties = new List<PropertyListing>();
                List<PropertyListing> siteProperties = _db.Properties
                    .Where(p => p.UserVars == "IMPORTED")
                    .AsNoTracking()
                    .ToList();

                foreach (Dictionary<string, string> data in properties)
                {
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = "Checking property (" + data["ADDRESS_1"] + ", " + data["ADDRESS_2"] + ") information from the BLM file...";
                    Lock.ReleaseWriterLock();

                    PropertyListing property = new PropertyListing();
                    await ProcessPropertyAsync(property, data, true);
                    feedProperties.Add(property);
                }

                // Now we have a list of properties, in the feed
                IEnumerable<PropertyListing> newProperties = feedProperties.Where(p => !siteProperties.Any(p2 => p2.Reference == p.Reference));
                IEnumerable<PropertyListing> existingProperties = siteProperties.Where(site => feedProperties.Any(feed => feed.Reference == site.Reference));
                IEnumerable<PropertyListing> extraneous = siteProperties.Where(p => !feedProperties.Any(p2 => p2.Reference == p.Reference));

                switch (_propertySettings.FTPImporterSettings.ExtraneousPropertyProcess)
                {
                    case ExtraneousPropertyProcess.StatusArchive:
                        extraneous = extraneous.Where(p => p.Status != ContentStatus.Archived);
                        break;
                    case ExtraneousPropertyProcess.StatusDelete:
                        extraneous = extraneous.Where(p => p.Status != ContentStatus.Deleted);
                        break;
                }

                Lock.AcquireWriterLock(Timeout.Infinite);
                ToAdd = newProperties.Count();
                ToDelete = extraneous.Count();
                ToUpdate = existingProperties.Count();
                Tasks = ToAdd + ToDelete + ToUpdate;
                Lock.ReleaseWriterLock();

                foreach (PropertyListing propertyRef in newProperties)
                {
                    CheckForCancel();
                    try
                    {
                        // Find matching record to update the property.
                        Dictionary<string, string> data = properties.SingleOrDefault(p => p["AGENT_REF"] == propertyRef.Reference);

                        PropertyListing property = await ProcessAsync(new PropertyListing(), data);

                        _db.Properties.Add(property);
                        await SaveChangesToDatabaseAsync();

                        Lock.AcquireWriterLock(Timeout.Infinite);
                        Processed++;
                        Added++;
                        Lock.ReleaseWriterLock();
                        MarkCompleteTask("Added new property: " + property.ToFormat(AddressFormat.SingleLine));
                    }
                    catch (Exception addPropertyException)
                    {
                        Lock.AcquireWriterLock(Timeout.Infinite);
                        Processed++;
                        Added++;
                        Errors.Add(FormatLog("Error adding property: " + addPropertyException.Message, propertyRef));
                        Lock.ReleaseWriterLock();
                        await _logService.AddExceptionAsync<BlmFileImporter>("An error occurred adding a property via BLM import.", addPropertyException);
                        MarkCompleteTask("Adding property failed: " + addPropertyException.Message);
                    }
                }
                await SaveChangesToDatabaseAsync();

                foreach (PropertyListing propertyRef in existingProperties)
                {
                    CheckForCancel();
                    Dictionary<string, string> data = properties.SingleOrDefault(p => p["AGENT_REF"] == propertyRef.Reference);

                    try
                    {
                        PropertyListing property = _db.Properties
                            .Include(p => p.Media)
                            .Include(p => p.FloorPlans)
                            .Include(p => p.Metadata)
                            .FirstOrDefault(p => p.Id == propertyRef.Id);
                        property = await ProcessAsync(property, data);
                        _db.Properties.Update(property);
                        await SaveChangesToDatabaseAsync();

                        Lock.AcquireWriterLock(Timeout.Infinite);
                        Processed++;
                        Updated++;
                        Lock.ReleaseWriterLock();
                        MarkCompleteTask("Updated property: " + property.ToFormat(AddressFormat.SingleLine));
                    }
                    catch (Exception updatePropertyException)
                    {
                        Lock.AcquireWriterLock(Timeout.Infinite);
                        Processed++;
                        Updated++;
                        Lock.ReleaseWriterLock();
                        await _logService.AddLogAsync<BlmFileImporter>("Error updating property: " + updatePropertyException.Message, FormatLog("Error updating property: " + updatePropertyException.Message, propertyRef), LogType.Error);
                        MarkCompleteTask("Updating property failed: " + updatePropertyException.Message);
                    }
                }
                await SaveChangesToDatabaseAsync();


                // Clean any from the DB that have been removed from the BLM file.
                foreach (PropertyListing property in extraneous)
                {
                    CheckForCancel();
                    try
                    {
                        switch (_propertySettings.FTPImporterSettings.ExtraneousPropertyProcess)
                        {
                            case ExtraneousPropertyProcess.Delete:
                                property.Media.ForEach(async m =>
                                {
                                    if (_propertySettings.FTPImporterSettings.DeletePhysicalImageFiles)
                                        try { await _media.DeleteStoredMedia(m); } catch (Exception) { }
                                    _db.Entry(m).State = EntityState.Deleted;

                                });
                                await SaveChangesToDatabaseAsync();
                                property.FloorPlans.ForEach(async m =>
                                {
                                    if (_propertySettings.FTPImporterSettings.DeletePhysicalImageFiles)
                                        try { await _media.DeleteStoredMedia(m); } catch (Exception) { }
                                    _db.Entry(m).State = EntityState.Deleted;

                                });
                                await SaveChangesToDatabaseAsync();
                                property.Metadata.ForEach(m =>
                                {
                                    _db.Entry(m).State = EntityState.Deleted;
                                });
                                await SaveChangesToDatabaseAsync();

                                _db.Entry(property).State = EntityState.Deleted;
                                await SaveChangesToDatabaseAsync();
                                break;

                            case ExtraneousPropertyProcess.StatusDelete:
                                property.Status = ContentStatus.Deleted;
                                _db.Properties.Update(property);
                                await SaveChangesToDatabaseAsync();
                                break;

                            case ExtraneousPropertyProcess.StatusArchive:
                                property.Status = ContentStatus.Archived;
                                _db.Properties.Update(property);
                                await SaveChangesToDatabaseAsync();
                                break;

                            case ExtraneousPropertyProcess.LeaseStatusLetSold:
                                if (property.ListingType == "Sale")
                                {
                                    property.LeaseStatus = "Sold";
                                }
                                else
                                {
                                    property.LeaseStatus = "Let";
                                }
                                _db.Properties.Update(property);
                                await SaveChangesToDatabaseAsync();
                                break;
                        }


                        Lock.AcquireWriterLock(Timeout.Infinite);
                        Processed++;
                        Deleted++;
                        Lock.ReleaseWriterLock();
                        MarkCompleteTask("Removed property: " + property.ToFormat(AddressFormat.SingleLine));
                    }
                    catch (Exception removePropertyException)
                    {
                        Lock.AcquireWriterLock(Timeout.Infinite);
                        Processed++;
                        Updated++;
                        Errors.Add(FormatLog("Error removing property: " + removePropertyException.Message, property));
                        Lock.ReleaseWriterLock();
                        await _logService.AddExceptionAsync<BlmFileImporter>("An error occurred updating a removing via BLM import.", removePropertyException);
                        MarkCompleteTask("Removing property failed: " + removePropertyException.Message);
                    }
                }
                await SaveChangesToDatabaseAsync();

                // Clean the temp directory...
                CleanTempFolder();

                Lock.AcquireWriterLock(Timeout.Infinite);
                Succeeded = true;
                Running = false;
                StatusMessage = "Update completed at " + DateTime.UtcNow.ToShortTimeString() + " on " + DateTime.UtcNow.ToLongDateString() + ".";
                Lock.ReleaseWriterLock();

                await _logService.AddLogAsync<BlmFileImporter>("Sucessfully imported properties via BLM.", "Process completed!", LogType.Success);

                return;
            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Running = false;
                StatusMessage = ex.Message;
                Lock.ReleaseWriterLock();
                await _logService.AddExceptionAsync<BlmFileImporter>("An error occurred importing properties via BLM.", ex);
                return;
            }
        }

        private void UnzipLocalFile()
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            StatusMessage = "Unzipping the local zipped data file (" + _propertySettings.FTPImporterSettings.ZipFile + "), please wait...";
            FileError = false;
            Lock.ReleaseWriterLock();
            // First thing is to get the zip file, and unzip the contents to the local folder.
            string zipFilename = Path.Combine(LocalFolder, _propertySettings.FTPImporterSettings.ZipFile);
            ZipFile zf = null;

            try
            {

                FileStream fs = File.OpenRead(zipFilename);
                zf = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }

                    string entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    string fullZipToPath = Path.Combine(LocalFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);

                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.

                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        private async Task<PropertyListing> ProcessAsync(PropertyListing property, Dictionary<string, string> data)
        {
            CheckForCancel();

            property = await ProcessPropertyAsync(property, data);
            property = await UpdateMetadataAsync(property, data);

            if (data != null)
            {
                try
                {
                    property = await ProcessImagesAsync(property, data);
                }
                catch (Exception ex)
                {
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = $"There was an error processing the images: {ex.Message}";
                    Errors.Add(FormatLog(StatusMessage, property));
                    Lock.ReleaseWriterLock();
                }

                try
                {
                    property = await ProcessFloorPlansAsync(property, data);
                }
                catch (Exception ex)
                {
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = $"There was an error processing the floor plans. {ex.Message}";
                    Errors.Add(FormatLog(StatusMessage, property));
                    Lock.ReleaseWriterLock();
                }

                try
                {
                    property = await ProcessDocumentsAsync(property, data);
                }
                catch (Exception ex)
                {
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = $"There was an error processing the info document. {ex.Message}";
                    Errors.Add(FormatLog(StatusMessage, property));
                    Lock.ReleaseWriterLock();
                }

            }

            return property;
        }
        private async Task<PropertyListing> ProcessDocumentsAsync(PropertyListing property, Dictionary<string, string> data)
        {
            // Documents
            foreach (string key in data.Keys.Where(k => k.Contains("MEDIA_DOCUMENT") && !k.Contains("TEXT")))
            {
                if (data[key].IsSet())
                {
                    // We have a document, download it with the FTPService
                    switch (_propertySettings.FTPImporterSettings.Method)
                    {
                        case PropertyImporterMethod.Directory:
                            await GetFileFromLocalAsync(data[key]);
                            break;
                        case PropertyImporterMethod.FtpBlm:
                            GetFileFromFtp(data[key]);
                            break;
                    }

                    if (!HasFileError())
                    {
                        Lock.AcquireWriterLock(Timeout.Infinite);
                        StatusMessage = "Processing document...";
                        Lock.ReleaseWriterLock();

                        string imageFile = TempFolder + data[key];
                        IMediaObject mediaResult = null;
                        FileInfo fi = new FileInfo(imageFile);
                        using (FileStream s = File.OpenRead(imageFile))
                        {
                            mediaResult = await _media.ProcessUpload(s, fi.Name, MimeTypes.GetMimeType(fi.Extension), fi.Length, DirectoryPath);
                        }
                        if (mediaResult != null)
                        {
                            if (property.InfoDownload == null)
                            {
                                // It's new, add the mediaitem to the site.
                                property.InfoDownload = mediaResult;
                            }
                            else
                            {
                                await _media.DeleteStoredMedia(new MediaObject(property.InfoDownload));
                                property.InfoDownload = property.InfoDownload.UpdateUrls(new PropertyMedia(mediaResult)) as MediaObject;
                            }
                        }
                    }
                }
            }
            return property;
        }
        private async Task<PropertyListing> ProcessFloorPlansAsync(PropertyListing property, Dictionary<string, string> data)
        {
            // Floor plans
            if (_propertySettings.FTPImporterSettings.ClearImagesBeforeImport)
            {
                if (property.FloorPlans != null)
                {
                    property.FloorPlans.ForEach(async m =>
                    {
                        if (_propertySettings.FTPImporterSettings.DeletePhysicalImageFiles)
                            await _media.DeleteStoredMedia(m);
                        _db.Entry(m).State = EntityState.Deleted;
                    });
                    await SaveChangesToDatabaseAsync();
                }
            }
            foreach (string key in data.Keys.Where(k => k.Contains("MEDIA_FLOOR_PLAN") && !k.Contains("TEXT")))
            {
                if (data[key].IsSet())
                {
                    // We have a floor plan reference, download it with the FTPService

                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = "Thumbnailing and processing floorplan...";
                    Lock.ReleaseWriterLock();

                    if (property.FloorPlans == null)
                    {
                        property.FloorPlans = new List<PropertyFloorplan>();
                    }

                    // if the photo exists, overwrite it.
                    PropertyFloorplan fp = property.FloorPlans.FirstOrDefault(f => f.Filename == data[key]);
                    if (fp == null)
                    {
                        // This image filename is not added to the property yet.

                        switch (_propertySettings.FTPImporterSettings.Method)
                        {
                            case PropertyImporterMethod.Directory:
                                await GetFileFromLocalAsync(data[key]);
                                break;
                            case PropertyImporterMethod.FtpBlm:
                                GetFileFromFtp(data[key]);
                                break;
                        }

                        if (!HasFileError())
                        {
                            string imageFile = TempFolder + data[key];
                            MediaObject mediaResult = null;
                            FileInfo fi = new FileInfo(imageFile);
                            using (FileStream s = File.OpenRead(imageFile))
                            {
                                mediaResult = await _media.ProcessUpload(s, fi.Name, MimeTypes.GetMimeType(fi.Extension), fi.Length, DirectoryPath) as MediaObject;
                            }
                            if (mediaResult != null)
                            {
                                fp = new PropertyFloorplan(mediaResult);
                                property.FloorPlans.Add(fp);
                                await SaveChangesToDatabaseAsync();
                            }
                        }
                    }
                }
            }
            return property;
        }
        private async Task<PropertyListing> ProcessImagesAsync(PropertyListing property, Dictionary<string, string> data)
        {
            // Images
            if (property.Media == null)
            {
                property.Media = new List<PropertyMedia>();
            }

            if (_propertySettings.FTPImporterSettings.ClearImagesBeforeImport)
            {
                if (property.Media != null)
                {
                    property.Media.ForEach(async m =>
                    {
                        if (_propertySettings.FTPImporterSettings.DeletePhysicalImageFiles)
                            await _media.DeleteStoredMedia(m);
                        _db.Entry(m).State = EntityState.Deleted;
                    });
                    await SaveChangesToDatabaseAsync();
                }
            }
            foreach (string key in data.Keys.Where(k => k.Contains("MEDIA_IMAGE") && !k.Contains("TEXT")).OrderBy(k => k))
            {
                try
                {
                    if (data[key].IsSet() && !key.Contains("60") && !key.Contains("61"))
                    {
                        Lock.AcquireWriterLock(Timeout.Infinite);
                        StatusMessage = $"Checking image ({data[key]})...";
                        Lock.ReleaseWriterLock();

                        // if the photo exists, overwrite it.
                        PropertyMedia fp = property.Media.FirstOrDefault(f => f.Filename == data[key]);
                        if (fp == null)
                        {
                            // This image filename is not added to the property yet.

                            switch (_propertySettings.FTPImporterSettings.Method)
                            {
                                case PropertyImporterMethod.Directory:
                                    await GetFileFromLocalAsync(data[key]);
                                    break;
                                case PropertyImporterMethod.FtpBlm:
                                    GetFileFromFtp(data[key]);
                                    break;
                            }

                            if (!HasFileError())
                            {

                                string imageFile = TempFolder + data[key];
                                Lock.AcquireWriterLock(Timeout.Infinite);
                                StatusMessage = $"Thumbnailing and processing image ({data[key]})...";
                                Lock.ReleaseWriterLock();
                                MediaObject mediaResult = null;
                                FileInfo fi = new FileInfo(imageFile);
                                using (FileStream s = File.OpenRead(imageFile))
                                {
                                    mediaResult = await _media.ProcessUpload(s, fi.Name, MimeTypes.GetMimeType(fi.Extension), fi.Length, DirectoryPath) as MediaObject;
                                }
                                if (mediaResult != null)
                                {
                                    fp = new PropertyMedia(mediaResult);
                                    property.Media.Add(fp);
                                    await SaveChangesToDatabaseAsync();
                                }
                            }
                        }
                        else
                        {
                            Lock.AcquireWriterLock(Timeout.Infinite);
                            StatusMessage = $"The image ({data[key]}) has already been added to property {property.Title} ({property.Id}), skipping upload & processing...";
                            Warnings.Add(FormatLog(StatusMessage));
                            Lock.ReleaseWriterLock();
                        }
                        if (property.FeaturedImageJson != null)
                        {
                            if (data[key] == property.FeaturedImage?.Filename)
                            {
                                property.FeaturedImage = property.FeaturedImage.UpdateUrls(fp) as MediaObject;
                            }
                        }
                        else
                        {
                            // add it.
                            property.FeaturedImage = fp;
                        }
                    }
                    else if (data[key].IsSet() && key.Contains("60"))
                    {
                        // We have an EPC, download it with the FTPService.
                        switch (_propertySettings.FTPImporterSettings.Method)
                        {
                            case PropertyImporterMethod.Directory:
                                await GetFileFromLocalAsync(data[key]);
                                break;
                            case PropertyImporterMethod.FtpBlm:
                                GetFileFromFtp(data[key]);
                                break;
                        }

                        if (!HasFileError())
                        {
                            Lock.AcquireWriterLock(Timeout.Infinite);
                            StatusMessage = "Thumbnailing and processing EPC...";
                            Lock.ReleaseWriterLock();

                            string imageFile = TempFolder + data[key];
                            MediaObject mediaResult = null;
                            FileInfo fi = new FileInfo(imageFile);
                            string fileName = data[key].ToLower().Replace(".jpg", ".pdf");
                            using (FileStream s = File.OpenRead(imageFile))
                            {
                                mediaResult = await _media.ProcessUpload(s, fileName, MimeTypes.GetMimeType("pdf"), fi.Length, DirectoryPath) as MediaObject;
                            }
                            if (mediaResult != null)
                            {
                                string url = mediaResult.Url;
                                if (!property.HasMeta("EnergyPerformanceCertificate"))
                                {
                                    property.AddMeta("EnergyPerformanceCertificate", url);
                                }
                                else
                                {
                                    property.UpdateMeta("EnergyPerformanceCertificate", url);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = $"There was an error processing an image ({data[key]}): {ex.Message}";
                    Errors.Add(FormatLog(StatusMessage, property));
                    Lock.ReleaseWriterLock();
                }
            }
            await SaveChangesToDatabaseAsync();
            return property;
        }

        /// <summary>
        /// Clears the temporary folder used for downloading remote data.
        /// </summary>
        private void CleanTempFolder()
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            StatusMessage = "Cleaning the temporary files folder...";
            Lock.ReleaseWriterLock();

            CheckForCancel();

            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(TempFolder);
            FileInfo[] oldFiles = directoryInfo.GetFiles();
            foreach (FileInfo fi in oldFiles)
            {
                // Delete the files in the directory. 
                fi.Delete();
            }
        }
        /// <summary>
        /// Validates the record to ensure that the detail stored is valid and can be used as a PropertyListing.
        /// </summary>
        /// <param name="propertyDetails">The record to validate.</param>
        /// <returns></returns>
        private async Task<bool> ValidatePropertyAsync(Dictionary<string, string> propertyDetails)
        {
            try
            {
                await ProcessPropertyAsync(new PropertyListing(), propertyDetails, true);
            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                StatusMessage = "There was an error with validating a property.";
                Warnings.Add(FormatLog(StatusMessage));
                Lock.ReleaseWriterLock();
                await _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                return false;
            }
            return true;
        }
        /// <summary>
        /// This will download the property file from the FTP service. The thread will wait until the file is downloaded before continuing.
        /// </summary>
        private async Task GetFileFromLocalAsync(string filename)
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            StatusMessage = "Copying the local file (" + filename + "), please wait...";
            FileError = false;
            Lock.ReleaseWriterLock();

            try
            {
                // Now simply copy the BLM file to the temp folder for the import.
                File.Copy(Path.Combine(LocalFolder, filename), Path.Combine(TempFolder, filename), true);

                Lock.AcquireWriterLock(Timeout.Infinite);
                StatusMessage = "Copied the file (" + filename + ") successfully...";
                Lock.ReleaseWriterLock();
            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                StatusMessage = "There was an error downloading the file (" + filename + ")...";
                Errors.Add(FormatLog(StatusMessage));
                FileError = true;
                Lock.ReleaseWriterLock();
                await _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
            }
        }
        /// <summary>
        /// This will download the property file from the FTP service. The thread will wait until the file is downloaded before continuing.
        /// </summary>
        private void GetFileFromFtp(string filename)
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            StatusMessage = "Downloading the file (" + filename + "), please wait...";
            FileError = false;
            Lock.ReleaseWriterLock();

            _ftp.GetFileFromFTP(_propertySettings.FTPImporterSettings.Server,
                                _propertySettings.FTPImporterSettings.Username,
                                _propertySettings.FTPImporterSettings.Password,
                                filename,
                                TempFolder);

            while (!_ftp.IsComplete())
            {
                CheckForCancel();
            }
            if (_ftp.Succeeded())
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                StatusMessage = "Downloaded the file (" + filename + ") successfully...";
                Lock.ReleaseWriterLock();
            }
            else
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                StatusMessage = "There was an error downloading the file (" + filename + ")...";
                Errors.Add(FormatLog(StatusMessage));
                FileError = true;
                Lock.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// Extracts the property data from the given BLM file, and translates it into a list of dictionaries of key/value pairs, representing each property.
        /// </summary>
        /// <returns></returns>
        private async Task<List<Dictionary<string, string>>> GetPropertiesFromFileAsync()
        {
            CheckForCancel();

            // Load the body template from the templates directory
            string fileName = TempFolder + _propertySettings.FTPImporterSettings.Filename;

            //// Get a StreamReader class that can be used to read the file
            StreamReader objStreamReader = new StreamReader(fileName);
            string fileContents = objStreamReader.ReadToEnd();
            objStreamReader.Close();

            string[] definitions = fileContents.ExtractTextBetween("#DEFINITION#", "#DATA#").Trim(Environment.NewLine.ToCharArray()).Trim().Split(new[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> defs = definitions.ToList();
            int imageTasks = defs.Count(c => c.Contains("MEDIA_IMAGE") && !c.Contains("TEXT"));
            int docTasks = defs.Count(c => c.Contains("MEDIA_FLOOR_PLAN") && !c.Contains("TEXT"));
            int fpTasks = defs.Count(c => c.Contains("MEDIA_DOCUMENT") && !c.Contains("TEXT"));

            string stringData = fileContents.ExtractTextBetween("#DATA#", "#END#").Trim(Environment.NewLine.ToCharArray()).Trim();
            string[] items = stringData.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

            // Now, read the entire file into a string
            List<Dictionary<string, string>> allProperties = new List<Dictionary<string, string>>();
            int counter = 1;
            foreach (string currentProperty in items.Select(p => p.Trim(Environment.NewLine.ToCharArray()).Trim()))
            {
                CheckForCancel();

                Lock.AcquireWriterLock(Timeout.Infinite);
                StatusMessage = "Processing raw property data from FTP BLM feed file (" + counter + ")...";
                Lock.ReleaseWriterLock();

                Dictionary<string, string> propertyDetails = GetPropertyDetails(currentProperty.Split('^'), definitions);
                if (!await ValidatePropertyAsync(propertyDetails))
                {
                    continue;
                }
                else
                {
                    allProperties.Add(propertyDetails);
                }

                counter++;
            }

            Lock.AcquireWriterLock(Timeout.Infinite);
            StatusMessage = "Downloaded the properties file, setting up the update process...";
            Lock.ReleaseWriterLock();

            return allProperties;
        }
        /// <summary>
        /// Extracts the property information to a key/value dictionary based on the array of definitions from the file.
        /// </summary>
        /// <param name="currentProperty">The property array to populate the dictionary values.</param>
        /// <param name="definitions">The definitions array to populate the dictionary keys.</param>
        /// <returns></returns>
        private Dictionary<string, string> GetPropertyDetails(string[] currentProperty, string[] definitions)
        {
            Dictionary<string, string> detail = new Dictionary<string, string>();
            for (int i = 0; i < definitions.Count(); i++)
            {
                detail.Add(definitions[i], currentProperty[i]);
            }
            return detail;
        }
        /// <summary>
        /// Takes the dictionary object and translates it into a full PropertyListing object, ready to insert to db.
        /// </summary>
        /// <param name="data">Property</param>
        /// <returns></returns>
        private async Task<PropertyListing> ProcessPropertyAsync(PropertyListing property, Dictionary<string, string> data, bool validatingOnly = false)
        {
            CheckForCancel();

            property.Status = data["PUBLISHED_FLAG"] == "1" ? ContentStatus.Published : ContentStatus.Archived;

            property.Reference = data["AGENT_REF"];

            property.LastEditedBy = User.UserName;
            property.LastEditedOn = DateTime.UtcNow;

            if (property.UserVars != "IMPORTED")
            {
                property.CreatedBy = User.UserName;
                property.CreatedOn = DateTime.UtcNow;
            }

            int bedrooms = 0;
            if (data.ContainsKey("BEDROOMS") && !int.TryParse(data["BEDROOMS"], out bedrooms))
            {
                bedrooms = 0;
            }

            property.Bedrooms = bedrooms;


            string furnished = PropertyDetails.Furnished[int.Parse(data["LET_FURN_ID"])];

            string propertyType = PropertyDetails.PropertyTypes[int.Parse(data["PROP_SUB_ID"])];
            property.PropertyType = propertyType;

            int transactionType = int.Parse(data["TRANS_TYPE_ID"]);
            if (transactionType == 1)
            {
                // Property is for sale, not rent.
                property.ListingType = "Sale";

                decimal price = 0;
                if (data.ContainsKey("PRICE") && decimal.TryParse(data["PRICE"], out price))
                {
                    property.AskingPrice = price;

                    string priceQual = PropertyDetails.PriceQualifiers[int.Parse(data["PRICE_QUALIFIER"])];
                    property.AskingPriceDisplay = priceQual;
                }
                else
                {
                    property.AskingPrice = price;
                    string priceQual = PropertyDetails.PriceQualifiers[0];
                    property.AskingPriceDisplay = priceQual;
                }
            }
            else
            {
                string listingType = PropertyDetails.ListingTypes[int.Parse(data["LET_TYPE_ID"])];
                property.ListingType = listingType;

                decimal rent = 0;
                if (data.ContainsKey("PRICE") && decimal.TryParse(data["PRICE"], out rent))
                {
                    property.Rent = rent;
                    string rentFreq = PropertyDetails.RentFrequency[int.Parse(data["LET_RENT_FREQUENCY"])];
                    property.RentDisplay = rentFreq;
                }
                else
                {
                    property.Rent = rent;
                    string rentFreq = PropertyDetails.RentFrequency[0];
                    property.RentDisplay = rentFreq;
                }

                decimal fees = 0;
                if (data.ContainsKey("LET_BOND") && decimal.TryParse(data["LET_BOND"], out fees))
                {
                    property.Fees = fees;
                    property.FeesDisplay = "{0} deposit";
                }
                else
                {
                    property.Fees = fees;
                    property.FeesDisplay = "{0}";
                }
            }

            // Whether it is sold/let etc.
            string leaseStatus = PropertyDetails.LeaseStatuses[int.Parse(data["STATUS_ID"])];
            property.LeaseStatus = leaseStatus;

            // Set planning as DwellingHouses as standard - as no planning information comes via BLM feed.
            property.Planning = "C3";

            // Update the PropertyListing object.
            property.Number = data["ADDRESS_1"];
            property.Address1 = data["ADDRESS_2"];
            property.Address2 = data.ContainsKey("ADDRESS_3") ? data["ADDRESS_3"] : "";
            property.AgentId = User.Id;
            property.AllowComments = false;
            property.AgentInfo = data.ContainsKey("ADMINISTRATION_FEE") ? data["ADMINISTRATION_FEE"] : "";
            property.City = data.ContainsKey("TOWN") ? data["TOWN"] : "";
            property.Confidential = false;
            property.Country = "United Kingdom";
            property.Description = data["DESCRIPTION"];
            property.Postcode = data["POSTCODE1"] + " " + data["POSTCODE2"];
            property.Public = true;
            property.PublishDate = DateTime.Now;
            property.ShareCount = 0;
            property.ShortDescription = data["SUMMARY"];
            property.Title = data.ContainsKey("DISPLAY_ADDRESS") ? data["DISPLAY_ADDRESS"] : property.Address1;
            property.Views = 0;

            property.UserVars = "IMPORTED";


            // Geocode
            if (!validatingOnly)
            {
                try
                {
                    GoogleAddress loc = _address.GeocodeAddress(property);
                    property.SetLocation(loc.Coordinates);
                }
                catch (GoogleGeocodingException ex)
                {
                    switch (ex.Status)
                    {
                        case GoogleStatus.RequestDenied:
                            Lock.AcquireWriterLock(Timeout.Infinite);
                            StatusMessage = "There was an error with the Google API [RequestDenied] this means your API account is not activated for Geocoding Requests.";
                            await _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                            Errors.Add(FormatLog(StatusMessage, property));
                            Lock.ReleaseWriterLock();
                            break;
                        case GoogleStatus.OverQueryLimit:
                            Lock.AcquireWriterLock(Timeout.Infinite);
                            StatusMessage = "There was an error with the Google API [OverQueryLimit] this means your API account is has run out of Geocoding Requests.";
                            await _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                            Errors.Add(FormatLog(StatusMessage, property));
                            Lock.ReleaseWriterLock();
                            break;
                        default:
                            Lock.AcquireWriterLock(Timeout.Infinite);
                            StatusMessage = "There was an error with the Google API [" + ex.Status.ToString() + "]: " + ex.Message;
                            await _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                            Errors.Add(FormatLog(StatusMessage, property));
                            Lock.ReleaseWriterLock();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = "There was an error GeoLocating the property.";
                    Errors.Add(FormatLog(StatusMessage, property));
                    Lock.ReleaseWriterLock();
                    await _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                }
            }

            return property;
        }
        private string FormatLog(string statusMessage, PropertyListing property = null)
        {
            if (property != null)
            {
                return string.Format("<strong>[{0} {1}] [Property {2} - {3}]</strong>: {4}", DateTime.UtcNow.ToShortDateString(), DateTime.UtcNow.ToShortTimeString(), property.Id, property.Postcode, statusMessage);
            }

            return string.Format("<strong>[{0} {1}]</strong>: {2}", DateTime.UtcNow.ToShortDateString(), DateTime.UtcNow.ToShortTimeString(), statusMessage);
        }
        private async Task<PropertyListing> UpdateMetadataAsync(PropertyListing property, Dictionary<string, string> data)
        {
            if (property.Metadata == null)
            {
                property.Metadata = new List<PropertyMeta>();
            }

            string[] format = { "yyyy-MM-dd hh:mm:ss" };
            if (DateTime.TryParseExact(data["LET_DATE_AVAILABLE"], format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime available))
            {
                if (!property.HasMeta("LetDate"))
                {
                    property.AddMeta("LetDate", available.ToString(), "System.DateTime");
                }
                else
                {
                    property.UpdateMeta("LetDate", available.ToString());
                }
            }


            string furnished = PropertyDetails.Furnished[int.Parse(data["LET_FURN_ID"])];
            if (!property.HasMeta("Furnished"))
            {
                property.AddMeta("Furnished", furnished);
            }
            else
            {
                property.UpdateMeta("Furnished", furnished);
            }

            string priceQual = PropertyDetails.PriceQualifiers[int.Parse(data["PRICE_QUALIFIER"])];
            if (!property.HasMeta("PriceQualifier"))
            {
                property.AddMeta("PriceQualifier", priceQual);
            }
            else
            {
                property.UpdateMeta("Furnished", furnished);
            }

            foreach (string key in data.Keys.Where(k => k.Contains("FEATURE")))
            {
                string metaKey = key.ToTitleCase();
                if (!property.HasMeta(metaKey))
                {
                    property.AddMeta(metaKey, data[key]);
                }
                else
                {
                    property.UpdateMeta(metaKey, data[key]);
                }
            }

            try
            {

                bool includesWater = data["LET_BILL_INC_WATER"] == "Y";
                property = AddMeta(property, "Bill.IncludesWater", JsonConvert.SerializeObject(includesWater), "System.Boolean");

                bool includesGas = data["LET_BILL_INC_GAS"] == "Y";
                property = AddMeta(property, "Bill.IncludesGas", JsonConvert.SerializeObject(includesGas), "System.Boolean");

                bool includesElectricity = data["LET_BILL_INC_ELECTRICITY"] == "Y";
                property = AddMeta(property, "Bill.IncludesElectricity", JsonConvert.SerializeObject(includesElectricity), "System.Boolean");

                bool includesTvLicense = data["LET_BILL_INC_TV_LICENCE"] == "Y";
                property = AddMeta(property, "Bill.IncludesTVLicense", JsonConvert.SerializeObject(includesTvLicense), "System.Boolean");

                bool includesTv = data["LET_BILL_INC_TV_SUBSCRIPTION"] == "Y";
                property = AddMeta(property, "Bill.IncludesTVSubscription", JsonConvert.SerializeObject(includesTv), "System.Boolean");

                bool includesInternet = data["LET_BILL_INC_INTERNET"] == "Y";
                property = AddMeta(property, "Bill.IncludesInternet", JsonConvert.SerializeObject(includesInternet), "System.Boolean");

            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                StatusMessage = "Could not get the bills inclusive data for the property.";
                Warnings.Add(FormatLog(StatusMessage, property));
                Lock.ReleaseWriterLock();
                await _logService.AddExceptionAsync<BlmFileImporter>(StatusMessage, property, ex, LogType.Warning);
            }
            return property;
        }
        private PropertyListing AddMeta(PropertyListing property, string key, string value, string type)
        {
            if (!property.HasMeta(key))
            {
                property.AddMeta(key, value, type);
            }
            else
            {
                property.UpdateMeta(key, value);
            }

            return property;
        }
        #region "Externals"

        public bool IsRunning()
        {
            Lock.AcquireReaderLock(Timeout.Infinite);
            bool running = Running;
            Lock.ReleaseReaderLock();
            return running;
        }
        public bool IsComplete()
        {
            Lock.AcquireReaderLock(Timeout.Infinite);
            bool running = Running;
            Lock.ReleaseReaderLock();
            return !running;
        }
        public void Kill()
        {
            // stop any threads belonging to this
            Lock.AcquireWriterLock(Timeout.Infinite);
            Cancelled = true;
            StatusMessage = "Cancelling...";
            Lock.ReleaseWriterLock();
            // stop the ftp service
            _ftp.Kill();
        }
        public PropertyImporterReport Report()
        {
            Lock.AcquireReaderLock(Timeout.Infinite);
            PropertyImporterReport report = new PropertyImporterReport
            {
                Added = Added,
                Complete = Succeeded ? 100 : Tasks > 0 ? (CompletedTasks / (double)Tasks) * 100 : 0,
                Deleted = Deleted,
                Processed = Processed,
                Running = Running,
                StatusMessage = StatusMessage,
                Total = Tasks,
                Updated = Updated,
                ToAdd = ToAdd,
                ToDelete = ToDelete,
                ToUpdate = ToUpdate,
                Errors = Errors,
                Warnings = Warnings
            };
            Lock.ReleaseReaderLock();
            return report;
        }

        #endregion

        #region "Helpers"

        /// <summary>
        /// Simply checks for the cancel flag, and throws an exception if true.
        /// </summary>
        private void MarkCompleteTask(string message = "")
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            if (message.IsSet())
            {
                StatusMessage = message;
            }

            CompletedTasks++;
            Lock.ReleaseWriterLock();
        }
        private void CheckForCancel()
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            _killFlag = Cancelled;
            Lock.ReleaseWriterLock();
            if (_killFlag)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Warnings.Add(FormatLog("Import process was cancelled early."));
                Lock.ReleaseWriterLock();
                throw new Exception("Action cancelled...");
            }
        }
        private bool HasFileError()
        {
            bool fileError = false;
            Lock.AcquireWriterLock(Timeout.Infinite);
            fileError = FileError;
            Lock.ReleaseWriterLock();
            return fileError;
        }
        private async Task<bool> SaveChangesToDatabaseAsync()
        {
            var saved = false;
            while (!saved)
            {
                try
                {
                    // Attempt to save changes to the database
                    await _db.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is PropertyListing || entry.Entity is PropertyMedia)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();

                            foreach (var property in proposedValues.Properties)
                            {
                                var proposedValue = proposedValues[property];
                                var databaseValue = databaseValues[property];

                                if (proposedValue != databaseValue)
                                {

                                }
                                // TODO: decide which value should be written to database
                                // proposedValues[property] = <value to be saved>;
                            }

                            // Refresh original values to bypass next concurrency check
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                        else
                        {
                            throw new NotSupportedException(
                                "Don't know how to handle concurrency conflicts for "
                                + entry.Metadata.Name);
                        }
                    }
                }
            }
            return saved;
        }
        #endregion

    }
}
