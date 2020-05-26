using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginApp
{
    internal class ArgParser
    {
        private Dictionary<string, List<string>> argValues;

        public ArgParser(string[] args, string[] validKeys)
        {
            argValues = ParseArgs(args, validKeys);
        }

        /// <summary>
        /// Returns the values if set or null if not
        /// </summary>
        /// <param name="key">The key to get the values for</param>
        /// <returns>Values or null</returns>
        public List<string> GetValues(string key)
        {
            if (argValues.ContainsKey(key))
            {
                return argValues[key];
            }

            return null;
        }

        /// <summary>
        /// Parse args in a dictionary of the argument (without '--') and a list of the provided values
        /// </summary>
        /// <param name="args">The args as entered</param>
        /// <param name="validKeys">Keys that are acceptable</param>
        /// <returns>The parsed args as dictionary</returns>
        public static Dictionary<string, List<string>> ParseArgs(string[] args, string[] validKeys)
        {
            Dictionary<string, List<string>> argValues = new Dictionary<string, List<string>>();

            string lastKey = null;
            for(int i = 0; i < args.Length; i++)
            {
                string currString = args[i];
                if(currString.StartsWith("--"))
                {
                    string key = currString.Split("--").Last();
                    if (!validKeys.Contains(key))
                    {
                        throw new ArgumentException(currString + " is not a valid argument");
                    }
                    lastKey = key;
                    argValues.Add(key, new List<string>());
                }
                else
                {
                    if(lastKey == null)
                    {
                        throw new ArgumentException("Cannot have a value without a flag - " + currString);
                    }
                    argValues[lastKey].Add(currString);
                }
            }
            return argValues;
        }
    }
}
