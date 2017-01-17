namespace Hood.Services
{
    public interface IFTPService
    {
        bool GetFileFromFTP(string server, string username, string password, string filename, string destination);
        bool IsComplete();
        bool Succeeded();
        void Kill();
        FTPServiceReport Report();
    }

    public class FTPServiceReport
    {
        public long BytesTransferred { get; set; }
        public double Complete { get; internal set; }
        public string StatusMessage { get; set; }
    }
}