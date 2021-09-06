using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Hood.Extensions
{
    public static class ViewDataDictionaryExtensions
    {
        public static bool IsSet<T>(this ViewDataDictionary<T> data, string key)
        {
            try
            {
                string str = data[key].ToString();
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