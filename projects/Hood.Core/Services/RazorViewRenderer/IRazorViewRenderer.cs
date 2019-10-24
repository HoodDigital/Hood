using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IRazorViewRenderer
    {
        Task<string> Render(string viewPath);
        Task<string> Render<TModel>(string viewPath, TModel model);
    }
}