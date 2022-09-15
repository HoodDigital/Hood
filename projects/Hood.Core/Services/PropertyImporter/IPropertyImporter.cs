using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IPropertyImporter
    {
        bool IsComplete();
        bool IsRunning();
        Task RunUpdate(HttpContext context, string userId, string userName);
        void Kill();
        PropertyImporterReport Report();
    }
}