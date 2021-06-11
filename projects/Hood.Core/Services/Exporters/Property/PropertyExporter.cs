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
using System.Net;
using System.IO.Compression;
using Hood.Core;

namespace Hood.Services
{
    public class PropertyExporter : IPropertyExporter
    {
        // Services
        private readonly IFTPService _ftp;
        private readonly IConfiguration _config;
        private readonly IMediaManager _media;
        private PropertySettings _propertySettings;
        private readonly IEmailSender _email;
        private HoodDbContext _db { get; set; }

        // Members
        private ReaderWriterLock Lock { get; set; }
        private PropertyExporterReport Status { get; set; }
        private string _tempFolder { get; set; }
        private string _contentFolder { get; set; }
        private bool _killFlag { get; set; }

        public PropertyExporter(IFTPService ftp, IWebHostEnvironment env, IConfiguration config, IMediaManager media, IEmailSender email)
        {
            _ftp = ftp;
            _config = config;
            Lock = new ReaderWriterLock();
            Status = new PropertyExporterReport()
            {
                Total = 0,
                Tasks = 0,
                CompletedTasks = 0,
                Processed = 0,
                PercentComplete = 0.0,
                Running = false,
                Cancelled = false,
                Succeeded = false,
                HasFile = false,
                Message = "Not running..."
            };
            _email = email;
            _propertySettings = Engine.Settings.Property;
            _media = media;
            _tempFolder = env.ContentRootPath + "\\Temporary\\" + typeof(PropertyExporter) + "\\";
            _contentFolder = _tempFolder + "Content\\";
        }

        #region "Properties"

        public bool ExportProperties(HttpContext context)
        {
            if (IsRunning())
                return false;

            try
            {
                ResetImporter(context);

                ThreadStart pts = new ThreadStart(ExportProperties);
                Thread thread = new Thread(pts)
                {
                    Name = "ImportContent",
                    Priority = ThreadPriority.Normal
                };
                thread.Start();

                return true;
            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Status.Running = false;
                Status.Message = ex.Message;
                Lock.ReleaseWriterLock();
                return false;
            }
        }
        private async void ExportProperties()
        {
            try
            {
                CleanTempFolder();

                // Get all the properties.

                List<PropertyListing> items = await _db.Properties
                    .AsNoTracking()
                    .ToListAsync();

                Lock.AcquireWriterLock(Timeout.Infinite);
                Status.Total = items.Count();
                Status.Tasks = Status.Total + 6;
                Lock.ReleaseWriterLock();

                List<PropertyListing> properties = new List<PropertyListing>();

                foreach (var item in items)
                {
                    try
                    {
                        var importItem = await _db.Properties
                            .Include(c => c.Media)
                            .Include(c => c.FloorPlans)
                            .Include(c => c.Metadata)
                            .FirstOrDefaultAsync(c => c.Id == item.Id);

                        // Save the FeaturedImage to the temporary directory as it's GUID.
                        if (importItem.FeaturedImage != null)
                        {
                            SaveFileToTemp(importItem.FeaturedImage.Url, importItem.FeaturedImage.UniqueId);
                            item.FeaturedImage = importItem.FeaturedImage;
                            item.FeaturedImage.Id = 0;
                        }
                        // Save the FeaturedImage to the temporary directory as it's GUID.
                        if (importItem.InfoDownload != null)
                        {
                            SaveFileToTemp(importItem.InfoDownload.Url, importItem.InfoDownload.UniqueId);
                            item.InfoDownload = importItem.InfoDownload;
                            item.InfoDownload.Id = 0;
                        }

                        if (importItem.Media != null && importItem.Media.Count > 0)
                            foreach (var media in importItem.Media)
                            {
                                SaveFileToTemp(media.Url, media.UniqueId);
                                var tempMedia = media;
                                tempMedia.Id = 0;
                                tempMedia.Property = null;
                                tempMedia.PropertyId = 0;
                                if (item.Media == null)
                                    item.Media = new List<PropertyMedia>();
                                item.Media.Add(tempMedia);
                            }

                        if (importItem.FloorPlans != null && importItem.FloorPlans.Count > 0)
                            foreach (var media in importItem.FloorPlans)
                            {
                                SaveFileToTemp(media.Url, media.UniqueId);
                                var tempMedia = media;
                                tempMedia.Id = 0;
                                tempMedia.Property = null;
                                tempMedia.PropertyId = 0;
                                if (item.FloorPlans == null)
                                    item.FloorPlans = new List<PropertyFloorplan>();
                                item.FloorPlans.Add(tempMedia);
                            }

                        if (importItem.Metadata != null && importItem.Metadata.Count > 0)
                            foreach (var meta in importItem.Metadata)
                            {
                                var tempMeta = meta;
                                tempMeta.Id = 0;
                                tempMeta.Property = null;
                                tempMeta.PropertyId = 0;
                                if (item.Metadata == null)
                                    item.Metadata = new List<PropertyMeta>();
                                item.Metadata.Add(tempMeta);
                            }

                        properties.Add(item);

                        Lock.AcquireWriterLock(Timeout.Infinite);
                        Status.Processed++;
                        Lock.ReleaseWriterLock();
                        MarkCompleteTask("Exported content: " + item.Title);
                    }
                    catch (Exception exportContentItemExeption)
                    {
                        Lock.AcquireWriterLock(Timeout.Infinite);
                        Status.Processed++;
                        Lock.ReleaseWriterLock();
                        MarkCompleteTask("Exporting property failed: " + exportContentItemExeption.Message);
                    }
                }

                var export = new
                {
                    Properties = properties
                };

                // Output the lot to json
                MarkCompleteTask("Outputting all content data to JSON...");
                File.WriteAllText(_contentFolder + "index.json", JsonConvert.SerializeObject(export));

                // Zip the file
                MarkCompleteTask("Zipping files...");
                string fileName = Guid.NewGuid().ToString() + ".zip";
                string zipTemp = Path.Combine(_tempFolder, fileName);
                ZipFile.CreateFromDirectory(_contentFolder, zipTemp);

                // Upload the zip with an expiry
                MarkCompleteTask("Publishing zip file...");
                string downloadUrl = "";
                using (Stream s = File.OpenRead(zipTemp))
                {
                    downloadUrl = await _media.UploadToSharedAccess(s, "Exports/" + fileName, DateTime.UtcNow.AddHours(24));
                }

                MarkCompleteTask("Cleaning up...");
                CleanTempFolder();

                // Send email to site email with the export link in it.
                MarkCompleteTask("Sending file link via email...");
                BasicSettings settings = Engine.Settings.Basic;
                if (settings.Email.IsSet())
                {
                    MailObject mail = new MailObject()
                    {
                        Subject = "Property Export",
                        PreHeader = "Your export was completed successfully"
                    };
                    mail.AddH3("Your export was completed successfully.", align: "left");
                    mail.AddParagraph("You have requested a property data export, it has completed successfully and the data can be downloaded from the following link:", align: "left");
                    mail.AddCallToAction("Download Zip", downloadUrl, align: "left");
                    mail.AddParagraph("And that's about it!", align: "left");
                    mail.To = new SendGrid.Helpers.Mail.EmailAddress(settings.Email);
                    mail.Template = MailSettings.SuccessTemplate;

                    await _email.SendEmailAsync(mail);
                }

                // Mark completed.
                Lock.AcquireWriterLock(Timeout.Infinite);
                Status.PercentComplete = 100;
                Status.Succeeded = true;
                Status.Running = false;
                Status.HasFile = true;
                Status.Message = "Property export completed at " + DateTime.UtcNow.ToShortTimeString() + " on " + DateTime.UtcNow.ToLongDateString() + ".";
                Status.Download = downloadUrl;
                Status.ExpireTime = DateTime.UtcNow.AddDays(1).ToDisplay();
                Lock.ReleaseWriterLock();
                return;
            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Status.Running = false;
                Status.Message = ex.Message;
                Lock.ReleaseWriterLock();
                return;
            }
        }

        #endregion

        #region "Externals"

        public bool IsComplete()
        {
            bool running = false;
            Lock.AcquireWriterLock(Timeout.Infinite);
            running = Status.Running;
            Lock.ReleaseWriterLock();
            return !running;
        }
        public void Kill()
        {
            // stop any threads belonging to this
            Lock.AcquireWriterLock(Timeout.Infinite);
            Status.Cancelled = true;
            Status.Message = "Cancelling...";
            Lock.ReleaseWriterLock();
            // stop the ftp service
            _ftp.Kill();
        }
        public PropertyExporterReport Report()
        {
            PropertyExporterReport report = new PropertyExporterReport();
            Lock.AcquireWriterLock(Timeout.Infinite);
            Status.PercentComplete = Status.Tasks == 0 ? 0 : ((double)Status.CompletedTasks / (double)Status.Tasks) * 100;
            report = Status;
            Lock.ReleaseWriterLock();
            return report;
        }

        #endregion

        #region "Helpers"

        private void CleanTempFolder()
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            Status.Message = "Cleaning the temporary files folder...";
            Lock.ReleaseWriterLock();

            CheckForCancel();

            if (!Directory.Exists(_tempFolder))
                Directory.CreateDirectory(_tempFolder);
            if (!Directory.Exists(_contentFolder))
                Directory.CreateDirectory(_contentFolder);

            DirectoryInfo directoryInfo = new DirectoryInfo(_contentFolder);
            FileInfo[] oldFiles = directoryInfo.GetFiles();
            foreach (FileInfo fi in oldFiles)
            {
                // Delete the files in the directory. 
                fi.Delete();
            }
            directoryInfo = new DirectoryInfo(_tempFolder);
            oldFiles = directoryInfo.GetFiles();
            foreach (FileInfo fi in oldFiles)
            {
                // Delete the files in the directory. 
                fi.Delete();
            }
        }

        private void ResetImporter(HttpContext context)
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            Status = new PropertyExporterReport()
            {
                Total = 0,
                Tasks = 0,
                CompletedTasks = 0,
                Processed = 0,
                PercentComplete = 0.0,
                Running = true,
                Cancelled = false,
                Succeeded = false,
                FileError = false,
                HasFile = false
            };
            Status.Message = "Starting import, loading property files from FTP Service...";
            _propertySettings = Engine.Settings.Property;

            // Get a new instance of the HoodDbContext for this import.
            var options = new DbContextOptionsBuilder<HoodDbContext>();
            options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
            _db = new HoodDbContext(options.Options);
            Lock.ReleaseWriterLock();
        }

        private bool IsRunning()
        {
            bool isRunning = false;
            Lock.AcquireWriterLock(Timeout.Infinite);
            isRunning = Status.Running;
            Lock.ReleaseWriterLock();
            return isRunning;
        }

        private void MarkCompleteTask(string message = "")
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            if (message.IsSet())
                Status.Message = message;
            Status.CompletedTasks++;
            Lock.ReleaseWriterLock();
        }

        private void CheckForCancel()
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            _killFlag = Status.Cancelled;
            Lock.ReleaseWriterLock();
            if (_killFlag)
                throw new Exception("Action cancelled...");
        }

        private void SaveFileToTemp(string url, string uniqueId)
        {
            if (!Directory.Exists(_contentFolder))
                Directory.CreateDirectory(_contentFolder);

            using (var client = new WebClient())
            {
                client.DownloadFile(url, _contentFolder + uniqueId);
            }
        }

        #endregion

    }
}
