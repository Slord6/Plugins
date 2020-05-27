using PluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace PluginApp
{
    class PluginHandler
    {
        public static List<IPlugin> LoadPlugins(string[] pluginPaths)
        {
            return ResolvePathsToLocal(pluginPaths).SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = LoadPluginAssembly(pluginPath);
                IEnumerable<IPlugin> pluginsFromAssembly = CreateAllPluginsInAssembly(pluginAssembly);
                foreach (IPlugin plugin in pluginsFromAssembly)
                {
                    plugin.OnLoad();
                }
                return pluginsFromAssembly;
            }).ToList();
        }

        private static IEnumerable<string> ResolvePathsToLocal(string[] pluginPaths)
        {
            return pluginPaths.ToList().Select<string, string>(pluginPath =>
            {
                if (PathTools.IsRemotePath(pluginPath))
                {
                    pluginPath = DownloadPlugin(pluginPath);
                    if (pluginPath == null)
                    {
                        Console.WriteLine("Failed to download " + pluginPath);
                        return null;
                    }
                }
                return pluginPath;
            }).Where(x => x != null).Distinct();
        }

        private static string DownloadPlugin(string path)
        {
            Console.WriteLine("Remote plugin - " + path);
            using (WebClient client = new WebClient())
            {
                string fileName = PathTools.UrlToFilename(path, "dll", @".\Plugins\");
                Console.WriteLine("Will save as " + fileName);
                if (File.Exists(fileName))
                {
                    Console.WriteLine("Will try to delete existing file");
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not delete - " + ex.Message);
                    }
                }

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

        public static void RunPlugins(ArgParser argParser, IEnumerable<IPlugin> plugins)
        {
            List<string> targetPlugins = argParser.GetValues(ArgStrings.TargetPlugins);
            if (targetPlugins == null)
            {
                targetPlugins = plugins.Select(p => p.Name).ToList();
            }

            foreach (string pluginName in targetPlugins)
            {
                Console.WriteLine($"-- {pluginName} --");

                IPlugin plugin = plugins.FirstOrDefault(c => c.Name == pluginName);
                if (plugin == null)
                {
                    Console.WriteLine("No such plugin is known.");
                    return;
                }

                Console.WriteLine("Running...");
                Task<PluginResultData> task = plugin.CreateTask();
                task.RunSynchronously();
                if (!task.IsCompletedSuccessfully)
                {
                    Console.WriteLine("Plugin failed");

                    Exception ex = task.Exception;
                    while (ex != null)
                    {
                        Console.WriteLine(ex.Message);
                        ex = ex.InnerException;
                    }
                }
                else
                {
                    Console.WriteLine("Plugin complete");
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
