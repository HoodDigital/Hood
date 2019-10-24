using System.Collections.Generic;

namespace Hood.Services
{
    public interface IThemesService
    {
        string CurrentTheme { get; }
        Theme Current { get; }
        List<Theme> Themes { get; }

        bool SetTheme(string themeName);
        bool IsDefault { get; }

        Theme Get(string theme);
    }
}