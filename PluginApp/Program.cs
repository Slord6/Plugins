using PluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace PluginApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ArgParser argParser;
                try
                {
                    argParser = new ArgParser(args, new string[]
                    {
                        ArgStrings.PluginLocations,
                        ArgStrings.Help,
                        ArgStrings.TargetPlugins,
                        ArgStrings.PluginLocationsFromFile
                    });
                }
                catch(ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(GetHelpText());
                    return;
                }

                if(argParser.GetValues(ArgStrings.Help) != null)
                {
                    Console.WriteLine(GetHelpText());
                    return;
                }

                List<string> pluginPaths = ResolvePluginPaths(argParser);

                // Load plugins and invoke OnLoad for each
                IEnumerable<IPlugin> plugins = PluginHandler.LoadPlugins(pluginPaths.ToArray());

                PluginHandler.RunPlugins(argParser, plugins);

                PluginHandler.UnloadPlugins(ref plugins);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static List<string> ResolvePluginPaths(ArgParser argParser)
        {
            // Get the plugins in the plugin folder
            List<string> pluginPaths = Directory.GetFiles(@".\Plugins\").ToList();

            // If other locations were passed add those (but add them in front so if they require re-downloading that happens first)
            List<string> locations = argParser.GetValues(ArgStrings.PluginLocations);
            if (locations != null)
            {
                locations.AddRange(pluginPaths);
                pluginPaths = locations;
            }

            // If there are files to load paths from, do that
            locations = argParser.GetValues(ArgStrings.PluginLocationsFromFile);
            if (locations != null)
            {
                List<string> loadedPaths = locations.SelectMany(filePath => LoadPathsFromFile(filePath)).ToList();
                // again insert in the front
                loadedPaths.AddRange(pluginPaths);
                pluginPaths = loadedPaths;
            }

            return pluginPaths;
        }

        private static List<string> LoadPathsFromFile(string filePath)
        {
            string fileName = filePath;
            if(PathTools.IsRemotePath(filePath))
            {
                string tempDir = @".\tmp\";
                if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

                fileName = PathTools.UrlToFilename(filePath, "txt", tempDir);
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(filePath, fileName);
                }
                Console.WriteLine("Downloaded " + filePath);
            }

            return File.ReadAllLines(fileName).ToList();
        }

        private static string GetHelpText()
        {
            return "HELP:" + Environment.NewLine +
                $"\t--{ArgStrings.Help} - This message" + Environment.NewLine +
                $"\t--{ArgStrings.PluginLocations} - List of locations to load plugins from, can be filepath or URI" + Environment.NewLine +
                $"\t--{ArgStrings.TargetPlugins} - List of plugin names to run" + Environment.NewLine +
                $"\t--{ArgStrings.PluginLocationsFromFile} - List of files from which to load plugin locations. Each location should be on its own line and may be remote";
        }
    }
}