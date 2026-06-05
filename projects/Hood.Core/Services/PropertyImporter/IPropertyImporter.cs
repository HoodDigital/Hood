using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IPropertyImporter
    {
        bool IsComplete();
        bool IsRunning();
        Task RunUpdate(string userId, string userName);
        void Kill();
        PropertyImporterReport Report();
    }
}
