using Microsoft.AspNetCore.Http;

namespace Hood.Services
{
    public interface IPropertyImporter
    {
        bool IsComplete();
        bool IsRunning();
        bool RunUpdate(HttpContext context);
        void Kill();
        PropertyImporterReport Report();
    }
}