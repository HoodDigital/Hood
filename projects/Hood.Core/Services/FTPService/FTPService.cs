using System;
using System.Threading;
using System.Net;
using System.IO;

namespace Hood.Services
{
    public class FTPService : IFTPService
    {

        private readonly ReaderWriterLock Lock;
        private bool Running;
        private double Complete;
        private byte[] Buffer;
        private long TotalBytes;
        private long BytesTransferred;
        private readonly int BufferSize;
        private string StatusMessage;
        private bool Cancelled;
        private bool Success;
        public FTPService()
        {
            Lock = new ReaderWriterLock();
            TotalBytes = 0;
            BytesTransferred = 0;
            Complete = 0.0;
            Running = true;
            Cancelled = false;
            Success = false;
            BufferSize = 2048;
            Buffer = new byte[BufferSize];
            StatusMessage = "Not running...";
        }

        public bool GetFileFromFTP(string server, string username, string password, string filename, string destination)
        {
            try
            {
                // First RESET the FTPService Class, clearing any previous data.
                // Remembering to lock the class before, and release it afterwards.
                Lock.AcquireWriterLock(Timeout.Infinite);
                TotalBytes = 0;
                BytesTransferred = 0;
                Complete = 0.0;
                Running = true;
                Cancelled = false;
                Buffer = new byte[BufferSize];
                StatusMessage = "Starting download, initialising FTP Service...";
                Lock.ReleaseWriterLock();

                // Create the newsletter objects and pass it to the threading function
                object[] parameters = { server, username, password, filename, destination };
                ParameterizedThreadStart pts = new ParameterizedThreadStart(GetFile);
                Thread thread = new Thread(pts)
                {
                    Name = "GetFile",
                    Priority = ThreadPriority.Normal
                };
                thread.Start(parameters);

                // If we have reached this point, then we have successfully started the thread, so return true.
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

        // SenderThread Function
        //
        // This function is the actual thread that will run alongside the main code, it will
        // iterate through every registered user, and send them a copy of the email.
        private void GetFile(object data)
        {
            bool cancelled = false;
            // Unpack the parameter objects.
            object[] parameters = (object[])data;
            string server = (string)parameters[0];
            string username = (string)parameters[1];
            string password = (string)parameters[2];
            string filename = (string)parameters[3];
            string destination = (string)parameters[4];

            // WHILE FILE IS NOT COMPLETELY DOWNLOADED AND FINISHED _ 
            // LOOP THE FTP PROCESS - OR WE HAVE TRIED 5 TIMES
            // IF AN ERROR OCCURS IT WILL RETRY, BASICALLY
            bool downloaded = false;
            int counter = 0;
            while (!downloaded && counter < 5)
            {
                FtpWebResponse response = null;
                FileStream outputStream = null;
                Stream ftpStream = null;
                try
                {
                    // Set the Total to 0, starting point.
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    BytesTransferred = 0;
                    cancelled = Cancelled;
                    Lock.ReleaseWriterLock();
                    if (cancelled)
                    {
                        throw new Exception("Action cancelled...");
                    }

                    outputStream = new FileStream(destination + filename, FileMode.OpenOrCreate);
                    FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(server + filename));
                    reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(username, password);
                    response = (FtpWebResponse)reqFTP.GetResponse();
                    ftpStream = response.GetResponseStream();

                    Lock.AcquireWriterLock(Timeout.Infinite);
                    TotalBytes = response.ContentLength;
                    StatusMessage = "Downloading file, " + filename + "...";
                    Lock.ReleaseWriterLock();

                    int readCount = 0;
                    readCount = ftpStream.Read(Buffer, 0, BufferSize);
                    BytesTransferred += readCount;
                    while (readCount > 0)
                    {
                        outputStream.Write(Buffer, 0, readCount);
                        readCount = ftpStream.Read(Buffer, 0, BufferSize);
                        BytesTransferred += readCount;
                        Complete = BytesTransferred / TotalBytes;
                        cancelled = Cancelled;
                        if (cancelled)
                        {
                            ftpStream.Close();
                            outputStream.Close();
                            response.Close();
                            throw new Exception("FTP action cancelled...");
                        }
                    }

                    ftpStream.Close();
                    outputStream.Close();
                    response.Close();

                    downloaded = true;

                    Lock.AcquireWriterLock(Timeout.Infinite);
                    Success = true;
                    StatusMessage = "Download of file, " + filename + " is complete.";
                    Lock.ReleaseWriterLock();

                }
                catch (Exception)
                {
                    downloaded = false;
                    counter++;
                    Lock.AcquireWriterLock(Timeout.Infinite);
                    StatusMessage = "Re-trying (" + counter + ") of file, " + filename + ".";
                    Lock.ReleaseWriterLock();
                }
                finally
                {
                    try
                    {
                        if (outputStream != null) outputStream.Close();
                    }
                    catch (Exception) { }
                    try
                    {
                        if (outputStream != null) outputStream.Close();
                    }
                    catch (Exception) { }
                    try
                    {
                        if (response != null) response.Close();
                    }
                    catch (Exception) { }
                }

            }

            Lock.AcquireWriterLock(Timeout.Infinite);
            Running = false;
            Lock.ReleaseWriterLock();

        }

        public bool IsComplete()
        {
            Lock.AcquireReaderLock(Timeout.Infinite);
            bool running = Running;
            Lock.ReleaseReaderLock();
            return !running;
        }

        public bool Succeeded()
        {
            Lock.AcquireReaderLock(Timeout.Infinite);
            bool succeeded = !Running && Success;
            Lock.ReleaseReaderLock();
            return succeeded;
        }

        public void Kill()
        {
            // stop any threads belonging to this
            Lock.AcquireWriterLock(Timeout.Infinite);
            Cancelled = true;
            StatusMessage = "Not running...";
            Lock.ReleaseWriterLock();
        }

        public FTPServiceReport Report()
        {
            Lock.AcquireReaderLock(Timeout.Infinite);
            FTPServiceReport report = new FTPServiceReport
            {
                Complete = Complete,
                StatusMessage = StatusMessage
            };
            Lock.ReleaseReaderLock();
            return report;
        }
    }
}
