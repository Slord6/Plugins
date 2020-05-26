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
                IEnumerable<IPlugin> plugins = LoadPlugins(pluginPaths.ToArray());

                RunPlugins(args, plugins);

                UnloadPlugins(ref plugins);

                Console.WriteLine("plugins not unloaded: " + plugins.Count());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static List<IPlugin> LoadPlugins(string[] pluginPaths)
        {
            return pluginPaths.SelectMany(pluginPath =>
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

        static void RunPlugins(string[] args, IEnumerable<IPlugin> plugins)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Commands: ");
                foreach (IPlugin command in plugins)
                {
                    Console.WriteLine($"{command.Name}\t - {command.Description}");
                }
            }
            else
            {
                foreach (string commandName in args)
                {
                    Console.WriteLine($"-- {commandName} --");

                    IPlugin command = plugins.FirstOrDefault(c => c.Name == commandName);
                    if (command == null)
                    {
                        Console.WriteLine("No such command is known.");
                        return;
                    }

                    command.CreateTask().RunSynchronously();

                    Console.WriteLine();
                }
            }
        }

        static void UnloadPlugins(ref IEnumerable<IPlugin> plugins)
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
                    $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}