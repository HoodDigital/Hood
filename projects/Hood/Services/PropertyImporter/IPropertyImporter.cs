using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IPropertyImporter
    {
        bool IsComplete();
        bool IsRunning();
        Task RunUpdate(HttpContext context);
        void Kill();
        PropertyImporterReport Report();
    }
}