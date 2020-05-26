using PluginBase;
using System;
using System.Threading.Tasks;

namespace Plugins
{
    public class HelloWorldPlugin : IPlugin
    {
        public string Name => "HelloWorld";

        public string Description => "Simple example plugin";

        public Task CreateTask()
        {
            return new Task(() =>
            {
                Console.WriteLine("HelloWorld Task!");
            });
        }

        public void OnLoad()
        {
            Console.WriteLine("Hello World plugin OnLoad");
        }

        public void OnUnload()
        {
            Console.WriteLine("Hello World plugin OnUnload");
        }
    }
}
