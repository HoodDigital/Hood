using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
