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
                if (args.Length == 1 && args[0] == "/d")
                {
                    Console.WriteLine("Waiting for any key...");
                    Console.ReadLine();
                }

                List<string> pluginPaths = Directory.GetFiles(@".\Plugins\").ToList();

                // Load plugins and invoke OnLoad for each
                IEnumerable<IPlugin> plugins = PluginHandler.LoadPlugins(pluginPaths.ToArray());

                PluginHandler.RunPlugins(args, plugins);

                PluginHandler.UnloadPlugins(ref plugins);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}