using Microsoft.AspNetCore.Http;

namespace Hood.Services
{
    public interface IContentExporter
    {
        bool IsComplete();
        bool ExportContent(HttpContext context);
        void Kill();
        ContentExporterReport Report();
    }

    public class ContentExporterReport
    {
        public int Total { get; set; }
        public int Processed { get; set; }
        public int Tasks { get; set; }
        public int CompletedTasks { get; set; }
        public double PercentComplete { get; set; }
        public string Message { get; set; }
        public string OutputFile { get; set; }
        public double Complete { get; set; }
        public bool Running { get; set; }
        public bool Cancelled { get; set; }
        public bool Succeeded { get; set; }
        public bool FileError { get; set; }
        public string Download { get; set; }
        public string ExpireTime { get; set; }
        public bool HasFile { get; set; }
    }
}