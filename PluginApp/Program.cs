using PluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                        ArgStrings.TargetPlugins
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

                List<string> pluginPaths = Directory.GetFiles(@".\Plugins\").ToList();
                List<string> locations = argParser.GetValues(ArgStrings.PluginLocations);
                if (locations != null)
                {
                    locations.AddRange(pluginPaths);
                    pluginPaths = locations;
                }

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

        private static string GetHelpText()
        {
            return "HELP:" + Environment.NewLine +
                "\t--help - This message" + Environment.NewLine +
                "\t--pluginLocations - List of locations to load plugins from, can be filepath or URI" + Environment.NewLine +
                "\t--targetPlugins - List of plugin names to run";
        }
    }
}