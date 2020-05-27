using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace PluginApp
{
    internal static class PathTools
    {
        public static bool IsRemotePath(string path)
        {
            try
            {
                Uri uri = new Uri(path);
                return (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttps);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Convert a url to a sensible file name
        /// </summary>
        /// <param name="url">The url to convert</param>
        /// <param name="extension">The extension of the resulting file (no period)</param>
        /// <param name="leadingPath">Additional path to be prepended to the file name</param>
        /// <returns>The file name</returns>
        public static string UrlToFilename(string url, string extension, string leadingPath = "")
        {
            // split on forward and backward slash taking the last element as this is most likely the file name
            // Then remove any query string by splittin on ? and taking the first
            // Then remove and reapply the extension to ensure there is only one
            string fileName = HttpUtility.UrlDecode(url)
                .Split("/", StringSplitOptions.RemoveEmptyEntries).Last()
                .Split(@"\", StringSplitOptions.RemoveEmptyEntries).Last()
                .Split("?", StringSplitOptions.RemoveEmptyEntries).First()
                .Split("." + extension, StringSplitOptions.RemoveEmptyEntries).First()
                + "." + extension;
            return leadingPath + ToValidFilename(fileName);
        }

        public static string ToValidFilename(string orig)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                orig = orig.Replace(c, '_');
            }
            return orig;
        }
    }
}
