using System;
using System.Linq;
using System.Threading;
using System.IO;
using Microsoft.AspNetCore.Http;
using Hood.Models;
using Hood.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Hood.Core;
using Hood.Interfaces;
using Hood.Caching;

namespace Hood.Services
{
    public class MediaRefreshService : IMediaRefreshService
    {
        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _env;
        private readonly IDirectoryManager _directoryManager;
        private IMediaManager _media;

        private HoodDbContext Database { get; set; }

        public MediaRefreshService(
            IHostingEnvironment env,
            IDirectoryManager directoryManager,
            IConfiguration config)
        {
            _config = config;
            Lock = new ReaderWriterLock();
            Total = 0;
            Processed = 0;
            PercentComplete = 0.0;
            Running = false;
            HasRun = false;
            Cancelled = false;
            Succeeded = false;
            StatusMessage = "Not running...";
            _env = env;
            _directoryManager = directoryManager;
            _media = new MediaManager(env);
            TempFolder = env.ContentRootPath + "\\Temporary\\" + typeof(MediaRefreshService) + "\\";
        }

        private ReaderWriterLock Lock { get; set; }
        private bool HasRun { get; set; }
        private bool Running { get; set; }
        private bool Succeeded { get; set; }
        private double PercentComplete { get; set; }
        private int Total { get; set; }
        private int Processed { get; set; }
        private string StatusMessage { get; set; }


        private bool Cancelled { get; set; }

        private bool _killFlag;
        private HttpContext _context;

        private string TempFolder { get; set; }

        public bool RunUpdate(HttpContext context)
        {
            try
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Total = 0;
                Processed = 0;
                PercentComplete = 0.0;
                Running = true;
                HasRun = true;
                Cancelled = false;
                Succeeded = false;
                StatusMessage = "Starting update...";
                _context = context;
                // Get a new instance of the HoodDbContext for this import.
                var options = new DbContextOptionsBuilder<HoodDbContext>();
                options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
                Database = new HoodDbContext(options.Options);

                _media = new MediaManager(_env);

                Lock.ReleaseWriterLock();

                ThreadStart pts = new ThreadStart(RefreshAllMedia);
                Thread thread = new Thread(pts)
                {
                    Name = "RefreshAllMedia",
                    Priority = ThreadPriority.Normal
                };
                thread.Start();

                return true;
            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Running = false;
                StatusMessage = ex.Message;
                Lock.ReleaseWriterLock();
                return false;
            }
        }

        /// <summary>
        /// This is the main thread, controls the whole update process. 
        /// All other functions are fired of from here and once complete, the update is complete.
        /// </summary>
        private async void RefreshAllMedia()
        {
            try
            {
                // Start by cleaning the temp folder.
                CleanTempFolder();

                var all = Database.Media;

                Lock.AcquireWriterLock(Timeout.Infinite);
                Total = all.Count();
                Lock.ReleaseWriterLock();

                foreach (var media in all)
                {
                    CheckForCancel();
                    try
                    {
                        await _media.RefreshMedia(media, TempFolder);
                    }
                    catch (WebException ex)
                    {
                        StatusMessage = string.Format("Downloading file failed: {0} - {1}", media.Filename, ex.Message);
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = string.Format("Error updating media: {0} -  {1}", media.Filename, ex.Message);
                    }

                    Lock.AcquireWriterLock(Timeout.Infinite);
                    Processed++;
                    PercentComplete = (Processed / Total) * 60;
                    StatusMessage = string.Format("Processed file: {0}", media.Filename);
                    Lock.ReleaseWriterLock();

                }
                Lock.AcquireWriterLock(Timeout.Infinite);
                PercentComplete = 60;
                StatusMessage = string.Format("Processed all files, commencing Content updates...");
                Lock.ReleaseWriterLock();

                await Database.SaveChangesAsync();
                // now we need to iterate through any items that may have media attached, and re-attach.
                // content
                // properties
                // app users

                foreach (var content in Database.Content)
                {
                    if (content.FeaturedImage != null)
                    {
                        var media = Database.Media.Find(content.FeaturedImage.Id);
                        content.FeaturedImage = new ContentMedia(media);
                        StatusMessage = string.Format("Refreshing content featured image: {0} {1}", content.Id, content.Title);
                    }
                }
                await Database.SaveChangesAsync();

                Lock.AcquireWriterLock(Timeout.Infinite);
                PercentComplete = 70;
                StatusMessage = string.Format("Content updated, commencing Properties updates...");
                Lock.ReleaseWriterLock();

                foreach (var property in Database.Properties)
                {
                    if (property.FeaturedImage != null)
                    {
                        property.FeaturedImage = Database.Media.Find(property.FeaturedImage.Id);
                        StatusMessage = string.Format("Refreshing property featured image: {0} {1}", property.Id, property.Title);
                    }
                    if (property.InfoDownload != null)
                    {
                        property.FeaturedImage = Database.Media.Find(property.FeaturedImage.Id);
                        StatusMessage = string.Format("Refreshing property info download: {0} {1}", property.Id, property.Title);
                    }
                }
                await Database.SaveChangesAsync();

                Lock.AcquireWriterLock(Timeout.Infinite);
                PercentComplete = 80;
                StatusMessage = string.Format("Properties updated, commencing Users updates...");
                Lock.ReleaseWriterLock();

                foreach (var user in Database.Users)
                {
                    if (user.Avatar != null)
                    {
                        user.Avatar = Database.Media.Find(user.Avatar.Id);
                        StatusMessage = string.Format("Refreshing user avatar: {0} {1}", user.Id, user.ToFullName());
                    }
                }
                await Database.SaveChangesAsync();

                Lock.AcquireWriterLock(Timeout.Infinite);
                PercentComplete = 90;
                StatusMessage = string.Format("All done, emailing results...");
                Lock.ReleaseWriterLock();

                MailObject message = new MailObject()
                {
                    PreHeader = "All media files have been refreshed.",
                    Subject = "All media files have been refreshed."
                };
                message.AddH1("Complete!");
                message.AddParagraph("All media files have been successfully refreshed on " + _context.GetSiteUrl());

                IEmailSender emailSender = Engine.Services.Resolve<IEmailSender>();
                await emailSender.NotifyRoleAsync(message, "SuperUser");

                // Clean the temp directory...
                CleanTempFolder();

                Lock.AcquireWriterLock(Timeout.Infinite);
                PercentComplete = 100;
                Succeeded = true;
                Running = false;
                StatusMessage = "Update completed at " + DateTime.Now.ToShortTimeString() + " on " + DateTime.Now.ToLongDateString() + ".";
                Lock.ReleaseWriterLock();

                return;
            }
            catch (Exception ex)
            {
                Lock.AcquireWriterLock(Timeout.Infinite);
                Running = false;
                StatusMessage = ex.Message;
                Lock.ReleaseWriterLock();
                return;
            }
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

        #region "Externals"

        public bool IsComplete()
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            bool running = Running;
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
        }
        public MediaRefreshReport Report()
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            var report = new MediaRefreshReport
            {
                Complete = PercentComplete,
                Succeeded = Succeeded,
                Processed = Processed,
                Running = Running,
                HasRun = HasRun,
                StatusMessage = StatusMessage,
                Total = Total
            };
            Lock.ReleaseWriterLock();
            return report;
        }

        #endregion

        #region "Helpers"
        private void CheckForCancel()
        {
            Lock.AcquireWriterLock(Timeout.Infinite);
            _killFlag = Cancelled;
            Lock.ReleaseWriterLock();
            if (_killFlag)
                throw new Exception("Action cancelled...");
        }
        #endregion

    }
}
