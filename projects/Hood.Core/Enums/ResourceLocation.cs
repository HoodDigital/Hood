namespace Hood.Enums
{
    public enum ResourceLocation
    {
        BeforeCss,
        AfterCss,
        BeforeJquery,
        BeforeVendors,
        Vendors,
        BeforeScripts,
        Scripts,
        AfterScripts
    }

    public static class ResourceLocationExtensions
    {
        public static bool Bundleable(this ResourceLocation location)
        {
            return location == ResourceLocation.Vendors || location == ResourceLocation.Scripts;
        }
    }
}
