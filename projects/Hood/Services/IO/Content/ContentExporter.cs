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

namespace Hood.Services
{
    public class ContentExporter : IContentExporter
    {
        // Services
        private IFTPService _ftp;
        private IConfiguration _config;
        private IMediaManager<SiteMedia> _media;
        private PropertySettings _propertySettings;
        private ISettingsRepository _settings;
        private IEmailSender _email;
        private HoodDbContext _db { get; set; }

        // Members
        private ReaderWriterLock Lock { get; set; }
        private ContentExporterReport Status { get; set; }
        private AccountInfo Account { get; set; }
        private string _tempFolder { get; set; }
        private string _contentFolder { get; set; }
        private bool _killFlag { get; set; }

        public ContentExporter(IFTPService ftp, IHostingEnvironment env, IConfiguration config, IMediaManager<SiteMedia> media, ISettingsRepository site, IEmailSender email)
        {
            _ftp = ftp;
            _config = config;
            Lock = new ReaderWriterLock();
            Status = new ContentExporterReport();
            Status.Total = 0;
            Status.Tasks = 0;
            Status.CompletedTasks = 0;
            Status.Processed = 0;
            Status.PercentComplete = 0.0;
            Status.Running = false;
            Status.Cancelled = false;
            Status.Succeeded = false;
            Status.HasFile = false;
            Status.Message = "Not running...";
            _settings = site;
            _email = email;
            _propertySettings = site.GetPropertySettings();
            _media = media;
            _tempFolder = env.ContentRootPath + "\\Temporary\\" + typeof(ContentExporter) + "\\";
            _contentFolder = _tempFolder + "Content\\";
        }

        #region "Content"

        public bool ExportContent(HttpContext context)
        {
            if (IsRunning())
                return false;

            try
            {
                ResetImporter(context);

                ThreadStart pts = new ThreadStart(ExportContent);
                Thread thread = new Thread(pts);
                thread.Name = "ImportContent";
                thread.Priority = ThreadPriority.Normal;
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

        private async void ExportContent()
        {
            try
            {
                CleanTempFolder();

                // Get all the properties.

                List<Content> items = await _db.Content
                    .AsNoTracking()
                    .ToListAsync();

                List<ContentCategory> categories = await _db.ContentCategories
                    .AsNoTracking()
                    .ToListAsync();

                Lock.AcquireWriterLock(Timeout.Infinite);
                Status.Total = items.Count();
                Status.Tasks = Status.Total + 6;
                Lock.ReleaseWriterLock();

                List<Content> content = new List<Content>();

                foreach (var item in items)
                {
                    try
                    {
                        var importItem = await _db.Content
                            .Include(c => c.Media)
                            .Include(c => c.Metadata)
                            .FirstOrDefaultAsync(c => c.Id == item.Id);

                        // Save the FeaturedImage to the temporary directory as it's GUID.
                        if (importItem.FeaturedImage != null)
                        {
                            SaveFileToTemp(importItem.FeaturedImage.Url, importItem.FeaturedImage.UniqueId);
                            item.FeaturedImage = importItem.FeaturedImage;
                            item.FeaturedImage.Id = 0;
                        }

                        if (importItem.Media != null && importItem.Media.Count > 0)
                            foreach (var media in importItem.Media)
                            {
                                SaveFileToTemp(media.Url, media.UniqueId);
                                var tempMedia = media;
                                tempMedia.Id = 0;
                                tempMedia.Content = null;
                                tempMedia.ContentId = 0;
                                if (item.Media == null)
                                    item.Media = new List<ContentMedia>();
                                item.Media.Add(tempMedia);
                            }

                        if (importItem.Metadata != null && importItem.Metadata.Count > 0)
                            foreach (var meta in importItem.Metadata)
                            {
                                var tempMeta = meta;
                                tempMeta.Id = 0;
                                tempMeta.Content = null;
                                tempMeta.ContentId = 0;
                                if (item.Metadata == null)
                                    item.Metadata = new List<ContentMeta>();
                                item.Metadata.Add(tempMeta);
                            }

                        content.Add(item);

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
                        MarkCompleteTask("Adding property failed: " + exportContentItemExeption.Message);
                    }
                }

                var export = new
                {
                    Categories = categories,
                    Content = content
                };

                // Output the lot to json
                MarkCompleteTask("Outputting all content data to JSON...");
                System.IO.File.WriteAllText(_contentFolder + "index.json", JsonConvert.SerializeObject(export));

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
                BasicSettings settings = _settings.GetBasicSettings();
                if (settings.Email.IsSet())
                {
                    MailObject mail = new MailObject();
                    mail.Subject = "Content Export";
                    mail.PreHeader = "Your export was completed successfully";
                    mail.AddH3("Your export was completed successfully.", align: "left");
                    mail.AddParagraph("You have requested a content data export, it has completed successfully and the data can be downloaded from the following link:", align: "left");
                    mail.AddCallToAction("Download Zip", downloadUrl, align: "left");
                    mail.AddParagraph("And that's about it!", align: "left");
                    mail.To = new SendGrid.Helpers.Mail.EmailAddress(settings.Email);
                    await _email.SendEmail(mail, MailSettings.SuccessTemplate);
                }

                // Mark completed.
                Lock.AcquireWriterLock(Timeout.Infinite);
                Status.PercentComplete = 100;
                Status.Succeeded = true;
                Status.Running = false;
                Status.HasFile = true;
                Status.Message = "Export completed at " + DateTime.Now.ToShortTimeString() + " on " + DateTime.Now.ToLongDateString() + ".";
                Status.Download = downloadUrl;
                Status.ExpireTime = DateTime.UtcNow.AddHours(24).ToDisplay();
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
        public ContentExporterReport Report()
        {
            ContentExporterReport report = new ContentExporterReport();
            Lock.AcquireWriterLock(Timeout.Infinite);
            Status.PercentComplete = Status.Tasks == 0 ? 0 : (Status.CompletedTasks / Status.Tasks) * 100;
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
            Status = new ContentExporterReport();
            Status.Total = 0;
            Status.Tasks = 0;
            Status.CompletedTasks = 0;
            Status.Processed = 0;
            Status.PercentComplete = 0.0;
            Status.Running = true;
            Status.Cancelled = false;
            Status.Succeeded = false;
            Status.FileError = false;
            Status.HasFile = false;
            Account = context.GetAccountInfo();
            Status.Message = "Starting import, loading property files from FTP Service...";
            _propertySettings = _settings.GetPropertySettings();

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
