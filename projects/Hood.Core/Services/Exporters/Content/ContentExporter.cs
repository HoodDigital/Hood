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
    public class ContentExporter : IContentExporter
    {
        // Services
        private readonly IFTPService _ftp;
        private readonly IConfiguration _config;
        private readonly IMediaManager _media;
        private readonly IEmailSender _email;
        private HoodDbContext _db;

        // Members
        private ReaderWriterLock Lock { get; set; }
        private ContentExporterReport Status { get; set; }
        private string TempFolder { get; set; }
        private string ContentFolder { get; set; }
        private bool KillFlag { get; set; }

        public ContentExporter(IFTPService ftp, IWebHostEnvironment env, IConfiguration config, IMediaManager media, IEmailSender email)
        {
            _ftp = ftp;
            _config = config;
            Lock = new ReaderWriterLock();
            Status = new ContentExporterReport()
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
            _media = media;
            TempFolder = env.ContentRootPath + "\\Temporary\\" + typeof(ContentExporter) + "\\";
            ContentFolder = TempFolder + "Content\\";
        }

        #region "Content"

        public bool ExportContent(HttpContext context)
        {
            if (IsRunning())
                return false;

            try
            {
                ResetImporter();

                ThreadStart pts = new ThreadStart(ExportContent);
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
                File.WriteAllText(ContentFolder + "index.json", JsonConvert.SerializeObject(export));

                // Zip the file
                MarkCompleteTask("Zipping files...");
                string fileName = Guid.NewGuid().ToString() + ".zip";
                string zipTemp = Path.Combine(TempFolder, fileName);
                ZipFile.CreateFromDirectory(ContentFolder, zipTemp);

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
                        Subject = "Content Export",
                        PreHeader = "Your export was completed successfully"
                    };
                    mail.AddH3("Your export was completed successfully.", align: "left");
                    mail.AddParagraph("You have requested a content data export, it has completed successfully and the data can be downloaded from the following link:", align: "left");
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
            Lock.AcquireWriterLock(Timeout.Infinite);
            bool running = Status.Running;
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
            Lock.AcquireWriterLock(Timeout.Infinite);
            Status.PercentComplete = Status.Tasks == 0 ? 0 : (Status.CompletedTasks / Status.Tasks) * 100;
            ContentExporterReport report = Status;
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

            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);
            if (!Directory.Exists(ContentFolder))
                Directory.CreateDirectory(ContentFolder);

            DirectoryInfo directoryInfo = new DirectoryInfo(ContentFolder);
            FileInfo[] oldFiles = directoryInfo.GetFiles();
            foreach (FileInfo fi in oldFiles)
            {
                // Delete the files in the directory. 
                fi.Delete();
            }
            directoryInfo = new DirectoryInfo(TempFolder);
            oldFiles = directoryInfo.GetFiles();
            foreach (FileInfo fi in oldFiles)
            {
                // Delete the files in the directory. 
                fi.Delete();
            }
        }

        private void ResetImporter()
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            Status = new ContentExporterReport()
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

            // Get a new instance of the HoodDbContext for this import.
            var options = new DbContextOptionsBuilder<HoodDbContext>();
            options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
            _db = new HoodDbContext(options.Options);
            Lock.ReleaseWriterLock();
        }

        private bool IsRunning()
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            bool isRunning = Status.Running;
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
            KillFlag = Status.Cancelled;
            Lock.ReleaseWriterLock();
            if (KillFlag)
                throw new Exception("Action cancelled...");
        }

        private void SaveFileToTemp(string url, string uniqueId)
        {
            if (!Directory.Exists(ContentFolder))
                Directory.CreateDirectory(ContentFolder);

            using (var client = new WebClient())
            {
                client.DownloadFile(url, ContentFolder + uniqueId);
            }
        }

        #endregion

    }
}
