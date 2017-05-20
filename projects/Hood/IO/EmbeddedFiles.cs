using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Linq;

namespace Hood.IO
{
    public static class EmbeddedFiles
    {
        /// <summary>
        /// Gets a directory listing from the embedded files in the Hood assembly. 
        /// WARNING, this should only be used for loading files in known definite locations. as the 
        /// file provider uses a flat structure, sub-directories will be returned along with the files.
        /// </summary>
        /// <param name="basePath">The base path in the form ~/path/of/the/file.extension</param>
        /// <returns></returns>
        public static string[] GetFiles(string basePath)
        {
            basePath = ReWritePath(basePath);
            var provider = GetProvider();
            var contents = provider.GetDirectoryContents("");
            var dir = contents.Where(p => p.Name.StartsWith(basePath));
            return dir.Select(f => f.Name.Replace(basePath, "")).ToArray();
        }

        public static EmbeddedFileProvider GetProvider()
        {

            // Get a reference to the assembly that contains the view components
            var assembly = typeof(Hood.HoodStartup).Assembly;
            // Create an EmbeddedFileProvider for that assembly
            var embeddedFileProvider = new EmbeddedFileProvider(
                assembly,
                "Hood"
            );
            return embeddedFileProvider;
        }

        public static string ReWritePath(string basePath, bool isDirectory = false)
        {
            if (basePath.StartsWith("~"))
                basePath = basePath.TrimStart('~');
            if (!basePath.EndsWith("/") && isDirectory)
                basePath = basePath + "/";

            basePath = basePath.Replace("/", ".");
            basePath = basePath.Replace("-", "_");

            if (basePath.StartsWith("."))
                basePath = basePath.TrimStart('.');

            return basePath;
        }

        public static string ReadAllText(string path)
        {
            path = ReWritePath(path);
            var provider = GetProvider();
            var file = provider.GetFileInfo(path);
            var contents = file.CreateReadStream();
            StreamReader s = new StreamReader(contents);
            return s.ReadToEnd();
        }
    }
}
