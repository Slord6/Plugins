using PluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PluginApp
{
    class PluginHandler
    {
        public static List<IPlugin> LoadPlugins(string[] pluginPaths)
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

        public static void RunPlugins(string[] args, IEnumerable<IPlugin> plugins)
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
                    $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}
