using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Hood.Extensions
{
    public static class ViewDataDictionaryExtensions
    {
        public static bool IsSet(this ViewDataDictionary<dynamic> data, string key)
        {
            try
            {
                string str = data[key] as string;
                if (str.IsSet())
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
