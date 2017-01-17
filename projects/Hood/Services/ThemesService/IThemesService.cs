using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Hood.Services
{
    public interface IThemesService
    {
        List<string> Themes { get; }
        Dictionary<string, IConfiguration> ThemeConfigs { get; }
        IConfiguration Config(string themeName);
        IConfiguration Current { get; }
        string CurrentTheme { get; }
    }
}