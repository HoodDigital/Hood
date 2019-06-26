using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using Microsoft.AspNetCore.Http;
using Hood.Models;
using Hood.Extensions;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Geocoding.Google;
using Hood.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Hood.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace Hood.Services
{
    public class BlmFileImporter : IPropertyImporter
    {
        private IFTPService _ftp;
        private IConfiguration _config;
        private IMediaManager<MediaObject> _media;
        private PropertySettings _propertySettings;
        private IHttpContextAccessor _context;

        public BlmFileImporter(
            IFTPService ftp,
            IHostingEnvironment env,
            IHttpContextAccessor context,
            IConfiguration config,
            IMediaManager<MediaObject> media,
            IAddressService address,
            ILogService logService)
        {
            _ftp = ftp;
            _config = config;
            Lock = new ReaderWriterLock();
            Total = 0;
            Tasks = 0;
            Processed = 0;
            Updated = 0;
            Added = 0;
            Deleted = 0;
            ToAdd = 0;
            ToDelete = 0;
            ToUpdate = 0;
            PercentComplete = 0.0;
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
            _media = media;
            _context = context;
            _address = address;
            _logService = logService;
        }

        private ReaderWriterLock Lock { get; set; }
        private bool Running { get; set; }
        private bool Succeeded { get; set; }
        private double PercentComplete { get; set; }
        private List<string> Errors { get; set; }
        private List<string> Warnings { get; set; }
        private int Total { get; set; }
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
        private HoodDbContext _db { get; set; }
        private bool _killFlag;
        private readonly IAddressService _address;
        private ILogService _logService;

        public bool RunUpdate(HttpContext context)
        {
            try
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Total = 0;
                Tasks = 0;
                CompletedTasks = 0;
                Processed = 0;
                Updated = 0;
                Added = 0;
                Deleted = 0;
                ToAdd = 0;
                ToDelete = 0;
                ToUpdate = 0;
                PercentComplete = 0.0;
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
                var options = new DbContextOptionsBuilder<HoodDbContext>();
                options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
                _db = new HoodDbContext(options.Options);

                var userManager = context.RequestServices.GetService<UserManager<ApplicationUser>>();
                User = userManager.FindByNameAsync("PropertyImporter").Result;
                if (User == null)
                {
                    var identityResult = userManager.CreateAsync(new ApplicationUser() { UserName = "PropertyImporter", Email = "importer@domain.con" }, Guid.NewGuid().ToString()).Result;
                    if (!identityResult.Succeeded)
                        throw new Exception("Could not load the PropertyImporter user account.");
                    User = userManager.FindByNameAsync("PropertyImporter").Result;
                    if (User == null)
                        throw new Exception("Could not load the PropertyImporter user account.");
                }

                Lock.ReleaseWriterLock();

                ThreadStart pts = new ThreadStart(Import);
                Thread thread = new Thread(pts)
                {
                    Name = "Import",
                    Priority = ThreadPriority.Normal
                };
                thread.Start();

                return true;
            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Running = false;
                _logService.AddExceptionAsync<BlmFileImporter>("An error occurred starting a property update.", ex);
                StatusMessage = ex.Message;
                Lock.ReleaseWriterLock();
                return false;
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
                if (_propertySettings.FTPImporterSettings.UseFTP)
                    GetFileFromFtp(_propertySettings.FTPImporterSettings.Filename);
                else
                {
                    if (_propertySettings.FTPImporterSettings.RequireUnzip)
                    {
                        UnzipLocalFile();
                    }

                    GetFileFromLocal(_propertySettings.FTPImporterSettings.Filename);
                }

                if (HasFileError())
                    throw new Exception("There was a problem downloading the properties file. Please try again.");

                // Go through the file, and extract key/value pairs of
                List<Dictionary<string, string>> properties = GetPropertiesFromFile();

                List<PropertyListing> feedProperties = new List<PropertyListing>();
                List<PropertyListing> siteProperties = _db.Properties
                    .Include(p => p.Media)
                    .Include(p => p.FloorPlans)
                    .Include(p => p.Metadata)
                    .Where(p => p.UserVars == "IMPORTED")
                    .AsNoTracking()
                    .ToList();

                foreach (Dictionary<string, string> data in properties)
                {
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = "Checking property (" + data["ADDRESS_1"] + ", " + data["ADDRESS_2"] + ") information from the BLM file...";
                    Lock.ReleaseWriterLock();

                    var property = new PropertyListing();
                    ProcessProperty(property, data, true);
                    feedProperties.Add(property);
                }

                // Now we have a list of properties, in the feed
                var newProperties = feedProperties.Where(p => !siteProperties.Any(p2 => p2.Reference == p.Reference));
                var existingProperties = siteProperties.Where(site => feedProperties.Any(feed => feed.Reference == site.Reference));
                var extraneous = siteProperties.Where(p => !feedProperties.Any(p2 => p2.Reference == p.Reference));

                Lock.AcquireWriterLock(Timeout.Infinite);
                ToAdd = newProperties.Count();
                ToDelete = extraneous.Count();
                ToUpdate = existingProperties.Count();
                Lock.ReleaseWriterLock();

                foreach (var propertyRef in newProperties)
                {
                    CheckForCancel();
                    try
                    {
                        // Find matching record to update the property.
                        Dictionary<string, string> data = properties.SingleOrDefault(p => p["AGENT_REF"] == propertyRef.Reference);

                        var property = await Process(new PropertyListing(), data);

                        _db.Properties.Add(property);
                        await _db.SaveChangesAsync();

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
                        await _logService.AddExceptionAsync<BlmFileImporter>("An error occurred adding a property via BLM import.", addPropertyException);
                        Lock.ReleaseWriterLock();
                        MarkCompleteTask("Adding property failed: " + addPropertyException.Message);
                    }
                }
                await _db.SaveChangesAsync();

                foreach (var propertyRef in existingProperties)
                {
                    CheckForCancel();
                    Dictionary<string, string> data = properties.SingleOrDefault(p => p["AGENT_REF"] == propertyRef.Reference);

                    try
                    {
                        var property = _db.Properties
                            .Include(p => p.Media)
                            .Include(p => p.FloorPlans)
                            .Include(p => p.Metadata)
                            .FirstOrDefault(p => p.Id == propertyRef.Id);
                        property = await Process(property, data);
                        _db.Properties.Update(property);
                        await _db.SaveChangesAsync();

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
                        await _logService.AddLogAsync<BlmFileImporter>("Error updating property: " + updatePropertyException.Message, FormatLog("Error updating property: " + updatePropertyException.Message, propertyRef), LogType.Error);
                        Lock.ReleaseWriterLock();
                        MarkCompleteTask("Updating property failed: " + updatePropertyException.Message);
                    }
                }
                await _db.SaveChangesAsync();


                // Clean any from the DB that have been removed from the BLM file.
                foreach (var property in extraneous)
                {
                    CheckForCancel();
                    try
                    {
                        // Property has been sold or let, mark it as such.
                        // It will be the user's responsibility to remove any old properties that are not required from the site manually.

                        if (property.ListingType == "Sale")
                        {
                            property.LeaseStatus = "Sold";
                        }
                        else
                        {
                            property.LeaseStatus = "Let";
                        }

                        _db.Properties.Update(property);
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
                        await _logService.AddExceptionAsync<BlmFileImporter>("An error occurred updating a removing via BLM import.", removePropertyException);
                        Lock.ReleaseWriterLock();
                        MarkCompleteTask("Removing property failed: " + removePropertyException.Message);
                    }
                }
                await _db.SaveChangesAsync();

                // Clean the temp directory...
                CleanTempFolder();

                Lock.AcquireWriterLock(Timeout.Infinite);
                PercentComplete = 100;
                Succeeded = true;
                Running = false;
                StatusMessage = "Update completed at " + DateTime.Now.ToShortTimeString() + " on " + DateTime.Now.ToLongDateString() + ".";
                Lock.ReleaseWriterLock();

                await _logService.AddLogAsync<BlmFileImporter>("Sucessfully imported properties via BLM.", "Process completed!", LogType.Success);

                return;
            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Running = false;
                await _logService.AddExceptionAsync<BlmFileImporter>("An error occurred importing properties via BLM.", ex);
                StatusMessage = ex.Message;
                Lock.ReleaseWriterLock();
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
            var zipFilename = Path.Combine(LocalFolder, _propertySettings.FTPImporterSettings.ZipFile);
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

                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(LocalFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);

                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

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

        private async Task<PropertyListing> Process(PropertyListing property, Dictionary<string, string> data)
        {
            CheckForCancel();

            property = ProcessProperty(property, data);
            property = await UpdateMetadataAsync(property, data);

            if (data != null)
            {
                try
                {
                    property = await ProcessImages(property, data);
                }
                catch (Exception ex)
                {
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = "There was an error processing the images...";
                    await _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                    Errors.Add(FormatLog(StatusMessage, property));
                    Lock.ReleaseWriterLock();
                }

                try
                {
                    property = await ProcessFloorPlans(property, data);
                }
                catch (Exception ex)
                {
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = "There was an error processing the floor plans..";
                    await _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                    Errors.Add(FormatLog(StatusMessage, property));
                    Lock.ReleaseWriterLock();
                }

                try
                {
                    property = await ProcessDocuments(property, data);
                }
                catch (Exception ex)
                {
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = "There was an error processing the info document...";
                    await _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                    Errors.Add(FormatLog(StatusMessage, property));
                    Lock.ReleaseWriterLock();
                }

            }

            return property;
        }

        private async Task<PropertyListing> ProcessDocuments(PropertyListing property, Dictionary<string, string> data)
        {
            // Documents
            foreach (string key in data.Keys.Where(k => k.Contains("MEDIA_DOCUMENT") && !k.Contains("TEXT")))
            {
                if (data[key].IsSet())
                {
                    // We have a document, download it with the FTPService
                    if (_propertySettings.FTPImporterSettings.UseFTP)
                        GetFileFromFtp(data[key]);
                    else
                        GetFileFromLocal(data[key]);

                    if (!HasFileError())
                    {
                        Lock.AcquireWriterLock(Timeout.Infinite);
                        StatusMessage = "Processing document...";
                        Lock.ReleaseWriterLock();

                        string imageFile = TempFolder + data[key];
                        MediaObject mediaResult = null;
                        FileInfo fi = new FileInfo(imageFile);
                        using (var s = File.OpenRead(imageFile))
                        {
                            mediaResult = await _media.ProcessUpload(s, fi.Name, MimeTypes.GetMimeType(fi.Extension), fi.Length, new MediaObject() { Directory = string.Format("property/{0}/", property.Id) });
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
                    MarkCompleteTask("Attached document successfully.");
                }
                else { MarkCompleteTask(); }
            }
            return property;
        }
        private async Task<PropertyListing> ProcessFloorPlans(PropertyListing property, Dictionary<string, string> data)
        {
            // Floor plans
            foreach (string key in data.Keys.Where(k => k.Contains("MEDIA_FLOOR_PLAN") && !k.Contains("TEXT")))
            {
                if (data[key].IsSet())
                {
                    // We have a floor plan reference, download it with the FTPService
                    if (_propertySettings.FTPImporterSettings.UseFTP)
                        GetFileFromFtp(data[key]);
                    else
                        GetFileFromLocal(data[key]);

                    if (!HasFileError())
                    {
                        Lock.AcquireWriterLock(Timeout.Infinite);
                        StatusMessage = "Thumbnailing and processing floorplan...";
                        Lock.ReleaseWriterLock();

                        string imageFile = TempFolder + data[key];
                        MediaObject mediaResult = null;
                        FileInfo fi = new FileInfo(imageFile);
                        using (var s = File.OpenRead(imageFile))
                        {
                            mediaResult = await _media.ProcessUpload(s, fi.Name, MimeTypes.GetMimeType(fi.Extension), fi.Length, new MediaObject() { Directory = "Property" });
                        }
                        if (mediaResult != null)
                        {
                            if (property.FloorPlans == null)
                                property.FloorPlans = new List<PropertyFloorplan>();

                            var fp = property.FloorPlans.FirstOrDefault(f => f.Filename == fi.Name);
                            if (fp != null)
                            {
                                await _media.DeleteStoredMedia(fp);
                                _db.Entry(fp).State = EntityState.Deleted;
                                await _db.SaveChangesAsync();
                            }

                            property.FloorPlans.Add(new PropertyFloorplan(mediaResult));
                            await _db.SaveChangesAsync();
                        }
                    }
                    MarkCompleteTask("Attached floor plan successfully.");
                }
                else { MarkCompleteTask(); }
            }
            return property;
        }
        private async Task<PropertyListing> ProcessImages(PropertyListing property, Dictionary<string, string> data)
        {
            // Images
            if (_propertySettings.FTPImporterSettings.ClearImagesBeforeImport)
            {
                if (property.Media != null)
                {
                    property.Media.ForEach(m =>
                    {
                        _db.Entry(m).State = EntityState.Deleted;
                    });
                    await _db.SaveChangesAsync();
                }
            }
            foreach (string key in data.Keys.Where(k => k.Contains("MEDIA_IMAGE") && !k.Contains("TEXT")).OrderBy(k => k))
            {
                if (data[key].IsSet() && !key.Contains("60") && !key.Contains("61"))
                {
                    // We have an image, download it with the FTPService
                    if (_propertySettings.FTPImporterSettings.UseFTP)
                        GetFileFromFtp(data[key]);
                    else
                        GetFileFromLocal(data[key]);

                    if (!HasFileError())
                    {
                        Lock.AcquireWriterLock(Timeout.Infinite);
                        StatusMessage = "Thumbnailing and processing image...";
                        Lock.ReleaseWriterLock();

                        string imageFile = TempFolder + data[key];
                        MediaObject mediaResult = null;
                        FileInfo fi = new FileInfo(imageFile);
                        using (var s = File.OpenRead(imageFile))
                        {
                            mediaResult = await _media.ProcessUpload(s, fi.Name, MimeTypes.GetMimeType(fi.Extension), fi.Length, new MediaObject() { Directory = "Property" });
                        }
                        if (mediaResult != null)
                        {
                            if (property.Media == null)
                                property.Media = new List<PropertyMedia>();

                            // if the photo exists, overwrite it.
                            var fp = property.Media.FirstOrDefault(f => f.Filename == fi.Name);
                            if (fp != null)
                            {
                                await _media.DeleteStoredMedia(fp);
                                property.Media.Remove(fp);
                                _db.Entry(fp).State = EntityState.Deleted;
                                await _db.SaveChangesAsync();
                            }

                            property.Media.Add(new PropertyMedia(mediaResult));

                            if (property.FeaturedImageJson != null)
                            {
                                if (data[key] == property.FeaturedImage?.Filename)
                                {
                                    property.FeaturedImage = property.FeaturedImage.UpdateUrls(mediaResult) as MediaObject;
                                }
                            }
                            else
                            {
                                // add it.
                                property.FeaturedImage = mediaResult;
                            }
                            await _db.SaveChangesAsync();
                        }
                    }
                    MarkCompleteTask("Attached image successfully.");
                }
                else if (data[key].IsSet() && key.Contains("60"))
                {
                    // We have an EPC, download it with the FTPService.
                    if (_propertySettings.FTPImporterSettings.UseFTP)
                        GetFileFromFtp(data[key]);
                    else
                        GetFileFromLocal(data[key]);

                    if (!HasFileError())
                    {
                        Lock.AcquireWriterLock(Timeout.Infinite);
                        StatusMessage = "Thumbnailing and processing EPC...";
                        Lock.ReleaseWriterLock();

                        string imageFile = TempFolder + data[key];
                        MediaObject mediaResult = null;
                        FileInfo fi = new FileInfo(imageFile);
                        string fileName = data[key].ToLower().Replace(".jpg", ".pdf");
                        using (var s = File.OpenRead(imageFile))
                        {
                            mediaResult = await _media.ProcessUpload(s, fileName, MimeTypes.GetMimeType("pdf"), fi.Length, new MediaObject() { Directory = "Property/Epc" });
                        }
                        if (mediaResult != null)
                        {
                            string url = mediaResult.Url;
                            if (!property.HasMeta("EnergyPerformanceCertificate"))
                                property.AddMeta(new PropertyMeta("EnergyPerformanceCertificate", url));
                            else
                                property.UpdateMeta("EnergyPerformanceCertificate", url);

                            var existing = _db.Media.SingleOrDefault(m => m.Filename == fileName);
                            if (existing == null)
                                _db.Media.Add(mediaResult);
                            else
                            {
                                _db.Media.Remove(existing);
                                _db.Media.Add(mediaResult);
                            }

                            await _db.SaveChangesAsync();
                        }
                    }
                    MarkCompleteTask("Attached image successfully.");
                }
                else { MarkCompleteTask(); }
            }
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
                Directory.CreateDirectory(TempFolder);

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
        private bool ValidateProperty(Dictionary<string, string> propertyDetails)
        {
            try
            {
                ProcessProperty(new PropertyListing(), propertyDetails, true);
            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                StatusMessage = "There was an error with validating a property.";
                _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                Warnings.Add(FormatLog(StatusMessage));
                Lock.ReleaseWriterLock();
                return false;
            }
            return true;
        }

        /// <summary>
        /// This will download the property file from the FTP service. The thread will wait until the file is downloaded before continuing.
        /// </summary>
        private void GetFileFromLocal(string filename)
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
                _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                Errors.Add(FormatLog(StatusMessage));
                FileError = true;
                Lock.ReleaseWriterLock();
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
        private List<Dictionary<string, string>> GetPropertiesFromFile()
        {
            CheckForCancel();

            // Load the body template from the templates directory
            string fileName = TempFolder + _propertySettings.FTPImporterSettings.Filename;

            //// Get a StreamReader class that can be used to read the file
            StreamReader objStreamReader = new StreamReader(fileName);
            string fileContents = objStreamReader.ReadToEnd();
            objStreamReader.Close();

            string[] definitions = fileContents.ExtractTextBetween("#DEFINITION#", "#DATA#").Trim(Environment.NewLine.ToCharArray()).Trim().Split(new[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            var defs = definitions.ToList();
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
                StatusMessage = "Processing raw property data from FTP BLM feed file (" + counter + " of " + Total + ")...";
                Lock.ReleaseWriterLock();

                Dictionary<string, string> propertyDetails = GetPropertyDetails(currentProperty.Split('^'), definitions);
                if (!ValidateProperty(propertyDetails))
                    continue;
                else
                    allProperties.Add(propertyDetails);

                MarkCompleteTask();
                counter++;
            }

            Lock.AcquireWriterLock(Timeout.Infinite);
            StatusMessage = "Downloaded the properties file, setting up the update process...";
            Total = allProperties.Count();
            Tasks = Total * (imageTasks + docTasks + fpTasks + 2) + 5;
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
        private PropertyListing ProcessProperty(PropertyListing property, Dictionary<string, string> data, bool validatingOnly = false)
        {
            CheckForCancel();

            property.Status = data["PUBLISHED_FLAG"] == "1" ? (int)Status.Published : (int)Status.Archived;

            property.Reference = data["AGENT_REF"];

            property.LastEditedBy = User.UserName;
            property.LastEditedOn = DateTime.Now;

            if (property.UserVars != "IMPORTED")
            {
                property.CreatedBy = User.UserName;
                property.CreatedOn = DateTime.Now;
            }

            int bedrooms = 0;
            if (data.ContainsKey("BEDROOMS") && !int.TryParse(data["BEDROOMS"], out bedrooms))
                bedrooms = 0;
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
            property.Additional = "";
            property.Number = data["ADDRESS_1"];
            property.Address1 = data["ADDRESS_2"];
            property.Address2 = data.ContainsKey("ADDRESS_3") ? data["ADDRESS_3"] : "";
            property.AgentId = User.Id;
            property.AllowComments = false;
            property.AgentInfo = data.ContainsKey("ADMINISTRATION_FEE") ? data["ADMINISTRATION_FEE"] : "";
            property.Areas = "";
            property.City = data.ContainsKey("TOWN") ? data["TOWN"] : "";
            property.Confidential = false;
            property.ContactName = "";
            property.Country = "United Kingdom";
            property.County = "";
            property.Description = data["DESCRIPTION"];
            property.Postcode = data["POSTCODE1"] + " " + data["POSTCODE2"];
            property.Public = true;
            property.PublishDate = DateTime.Now;
            property.ShareCount = 0;
            property.ShortDescription = data["SUMMARY"];
            property.Size = "";
            property.SystemNotes = "";
            property.Tags = "";
            property.Title = data.ContainsKey("DISPLAY_ADDRESS") ? data["DISPLAY_ADDRESS"] : property.Address1;
            property.Views = 0;

            property.UserVars = "IMPORTED";


            // Geocode
            if (!validatingOnly)
            {
                try
                {
                    var loc = _address.GeocodeAddress(property);
                    property.SetLocation(loc.Coordinates);

                    // check for missing address elements
                    try
                    {
                        if (!property.Address2.IsSet())
                            property.Address2 = loc.Components.FirstOrDefault(a => a.Types.Contains(GoogleAddressType.Route)).LongName;

                        if (!property.County.IsSet())
                            property.County = loc.Components.FirstOrDefault(a => a.Types.Contains(GoogleAddressType.AdministrativeAreaLevel2)).LongName;

                        if (!property.City.IsSet())
                            property.City = loc.Components.FirstOrDefault(a => a.Types.Contains(GoogleAddressType.PostalTown)).LongName;
                    }
                    catch (Exception)
                    {

                    }
                }
                catch (GoogleGeocodingException ex)
                {
                    switch (ex.Status)
                    {
                        case GoogleStatus.RequestDenied:
                            Lock.AcquireWriterLock(Timeout.Infinite);
                            StatusMessage = "There was an error with the Google API [RequestDenied] this means your API account is not activated for Geocoding Requests.";
                            _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                            Errors.Add(FormatLog(StatusMessage, property));
                            Lock.ReleaseWriterLock();
                            break;
                        case GoogleStatus.OverQueryLimit:
                            Lock.AcquireWriterLock(Timeout.Infinite);
                            StatusMessage = "There was an error with the Google API [OverQueryLimit] this means your API account is has run out of Geocoding Requests.";
                            _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                            Errors.Add(FormatLog(StatusMessage, property));
                            Lock.ReleaseWriterLock();
                            break;
                        default:
                            Lock.AcquireWriterLock(Timeout.Infinite);
                            StatusMessage = "There was an error with the Google API [" + ex.Status.ToString() + "]: " + ex.Message;
                            _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                            Errors.Add(FormatLog(StatusMessage, property));
                            Lock.ReleaseWriterLock();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = "There was an error GeoLocating the property.";
                    _logService.AddExceptionAsync<BlmFileImporter>("BLM Property Importer: " + StatusMessage, ex);
                    Errors.Add(FormatLog(StatusMessage, property));
                    Lock.ReleaseWriterLock();
                }
            }

            return property;
        }

        private string FormatLog(string statusMessage, PropertyListing property = null)
        {
            if (property != null)
                return string.Format("<strong>[{0} {1}] [Property {2} - {3}]</strong>: {4}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), property.Id, property.Postcode, statusMessage);
            return string.Format("<strong>[{0} {1}]</strong>: {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), statusMessage);
        }

        private async Task<PropertyListing> UpdateMetadataAsync(PropertyListing property, Dictionary<string, string> data)
        {
            if (property.Metadata == null)
                property.Metadata = new List<PropertyMeta>();

            string[] format = { "yyyy-MM-dd hh:mm:ss" };
            DateTime available = DateTime.Now;

            if (DateTime.TryParseExact(data["LET_DATE_AVAILABLE"], format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime date))
                available = date;

            if (!property.HasMeta("LetDate"))
                property.AddMeta(new PropertyMeta() { Name = "LetDate", BaseValue = JsonConvert.SerializeObject(available), Type = "System.DateTime" });
            else
                property.UpdateMeta("LetDate", available);


            string furnished = PropertyDetails.Furnished[int.Parse(data["LET_FURN_ID"])];
            if (!property.HasMeta("Furnished"))
                property.AddMeta(new PropertyMeta("Furnished", furnished));
            else
                property.UpdateMeta("Furnished", furnished);

            string priceQual = PropertyDetails.PriceQualifiers[int.Parse(data["PRICE_QUALIFIER"])];
            if (!property.HasMeta("PriceQualifier"))
                property.AddMeta(new PropertyMeta("PriceQualifier", priceQual));
            else
                property.UpdateMeta("Furnished", furnished);

            foreach (string key in data.Keys.Where(k => k.Contains("FEATURE")))
            {
                string metaKey = key.ToTitleCase();
                if (!property.HasMeta(metaKey))
                    property.AddMeta(new PropertyMeta(metaKey, data[key]));
                else
                    property.UpdateMeta(metaKey, data[key]);
            }

            try
            {

                bool includesWater = data["LET_BILL_INC_WATER"] == "Y";
                property = AddMeta(property, "Bill.Includes.Water", JsonConvert.SerializeObject(includesWater), "System.Boolean");

                bool includesGas = data["LET_BILL_INC_GAS"] == "Y";
                property = AddMeta(property, "Bill.Includes.Gas", JsonConvert.SerializeObject(includesGas), "System.Boolean");

                bool includesElectricity = data["LET_BILL_INC_ELECTRICITY"] == "Y";
                property = AddMeta(property, "Bill.Includes.Electricity", JsonConvert.SerializeObject(includesElectricity), "System.Boolean");

                bool includesTvLicense = data["LET_BILL_INC_TV_LICENCE"] == "Y";
                property = AddMeta(property, "Bill.Includes.TV.License", JsonConvert.SerializeObject(includesTvLicense), "System.Boolean");

                bool includesTv = data["LET_BILL_INC_TV_SUBSCRIPTION"] == "Y";
                property = AddMeta(property, "Bill.Includes.TV.Subscription", JsonConvert.SerializeObject(includesTv), "System.Boolean");

                bool includesInternet = data["LET_BILL_INC_INTERNET"] == "Y";
                property = AddMeta(property, "Bill.Includes.Internet", JsonConvert.SerializeObject(includesInternet), "System.Boolean");

            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                StatusMessage = "Could not get the letting data for the property, this could be a sale property.";
                await _logService.AddExceptionAsync<BlmFileImporter>(StatusMessage, property, ex);
                Warnings.Add(FormatLog(StatusMessage, property));
                Lock.ReleaseWriterLock();
            }
            return property;
        }

        private PropertyListing AddMeta(PropertyListing property, string key, string value, string type)
        {
            if (!property.HasMeta(key))
                property.AddMeta(new PropertyMeta(key, value, type));
            else
                property.UpdateMeta(key, value);
            return property;
        }

        #region "Externals"

        public bool IsRunning()
        {
            bool running = false;
            Lock.AcquireWriterLock(Timeout.Infinite);
            running = Running;
            Lock.ReleaseWriterLock();
            return running;
        }
        public bool IsComplete()
        {
            bool running = false;
            Lock.AcquireWriterLock(Timeout.Infinite);
            running = Running;
            Lock.ReleaseWriterLock();
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
            PropertyImporterReport report = new PropertyImporterReport();
            Lock.AcquireWriterLock(Timeout.Infinite);
            report = new PropertyImporterReport
            {
                Added = Added,
                Complete = Succeeded ? 100 : Tasks > 0 ? ((double)CompletedTasks / (double)Tasks) * (double)100 : 0,
                Deleted = Deleted,
                Processed = Processed,
                Running = Running,
                StatusMessage = StatusMessage,
                Total = Total,
                Updated = Updated,
                ToAdd = ToAdd,
                ToDelete = ToDelete,
                ToUpdate = ToUpdate,
                Errors = Errors,
                Warnings = Warnings
            };
            Lock.ReleaseWriterLock();
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
                StatusMessage = message;
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
        #endregion

    }
}
