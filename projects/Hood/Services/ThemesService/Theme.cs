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
        public string Name { get => _config["Name"].ToString(); }
        public string FullName { get => _config["FullName"].ToString(); }
        public string Author { get => _config["Author"].ToString(); }
        public string PreviewImage { get => _config["PreviewImage"].ToString(); }
        public bool IsActive
        {
            get
            {
                return Engine.Themes.Current.Name == Name;
            }
        }
    }
}