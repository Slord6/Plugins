using PluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;

namespace PluginApp
{
    class PluginHandler
    {
        public static List<IPlugin> LoadPlugins(string[] pluginPaths)
        {
            return pluginPaths.SelectMany(pluginPath =>
            {
                if(IsRemotePath(pluginPath))
                {
                    pluginPath = DownloadPlugin(pluginPath);
                    if(pluginPath == null)
                    {
                        Console.WriteLine("Failed to download " + pluginPath);
                        return new List<IPlugin>();
                    }
                }

                Assembly pluginAssembly = LoadPluginAssembly(pluginPath);
                IEnumerable<IPlugin> pluginsFromAssembly = CreateAllPluginsInAssembly(pluginAssembly);
                foreach (IPlugin plugin in pluginsFromAssembly)
                {
                    plugin.OnLoad();
                }
                return pluginsFromAssembly;
            }).ToList();
        }

        private static bool IsRemotePath(string path)
        {
            try
            {
                Uri uri = new Uri(path);
                return (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttps);
            }
            catch (Exception) {
                return false;
            }
        }

        private static string DownloadPlugin(string path)
        {
            Console.WriteLine("Remote plugin - " + path);
            using (WebClient client = new WebClient())
            {
                string fileName = UrlToAssemblyFileName(path);
                Console.WriteLine("Will save as " + fileName);
                try
                {
                    client.DownloadFile(path, fileName);
                    return fileName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to download plugin from " + path);
                    Console.WriteLine(ex.Message);
                    if(ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                    return null;
                }
            }
        }

        private static string UrlToAssemblyFileName(string url)
        {
            string fileName = HttpUtility.UrlDecode(url)
                .Split("/", StringSplitOptions.RemoveEmptyEntries).Last()
                .Split(@"\", StringSplitOptions.RemoveEmptyEntries).Last()
                .Split("?", StringSplitOptions.RemoveEmptyEntries).First()
                .Split(".dll", StringSplitOptions.RemoveEmptyEntries).First()
                +".dll";
            return @".\Plugins\" + ToValidFilename(fileName);
        }

        private static string ToValidFilename(string orig)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                orig = orig.Replace(c, '_');
            }
            return orig;
        }

        public static void RunPlugins(ArgParser argParser, IEnumerable<IPlugin> plugins)
        {
            List<string> targetPlugins = argParser.GetValues(ArgStrings.TargetPlugins);
            if (targetPlugins == null)
            {
                Console.WriteLine("Loaded Plugins: ");
                foreach (IPlugin command in plugins)
                {
                    Console.WriteLine($"{command.Name}\t - {command.Description}");
                }
            }
            else
            {
                foreach (string pluginName in targetPlugins)
                {
                    Console.WriteLine($"-- {pluginName} --");

                    IPlugin plugin = plugins.FirstOrDefault(c => c.Name == pluginName);
                    if (plugin == null)
                    {
                        Console.WriteLine("No such command is known.");
                        return;
                    }

                    plugin.CreateTask().RunSynchronously();

                    Console.WriteLine();
                }
            }
        }

        public static void UnloadPlugins(ref IEnumerable<IPlugin> plugins)
        {
            foreach (IPlugin plugin in plugins)
            {
                plugin.OnUnload();
            }
            plugins = new List<IPlugin>();
        }

        static Assembly LoadPluginAssembly(string pluginLocation)
        {
            Console.WriteLine($"Loading plugins from: {pluginLocation}");
            PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        }

        static IEnumerable<IPlugin> CreateAllPluginsInAssembly(Assembly assembly)
        {
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type))
                {
                    IPlugin result = Activator.CreateInstance(type) as IPlugin;
                    if (result != null)
                    {
                        count++;
                        yield return result;
                    }
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements IPlugin in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}
