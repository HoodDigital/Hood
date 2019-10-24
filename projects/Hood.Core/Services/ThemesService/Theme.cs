using Hood.Core;
using Microsoft.Extensions.Configuration;

namespace Hood.Services
{
    public class Theme
    {
        private readonly IConfiguration _config;
        public Theme(IConfiguration config)
        {
            _config = config;
        }
        public string Name { get => _config["Name"]; }
        public string FullName { get => _config["FullName"]; }
        public string Author { get => _config["Author"]; }
        public string Description { get => _config["Description"]; }
        public string PreviewImage { get => _config["PreviewImage"]; }
        public string UI { get => _config["UI"]; }
        public bool IsActive
        {
            get
            {
                return Engine.Themes.Current.Name == Name;
            }
        }
    }
}