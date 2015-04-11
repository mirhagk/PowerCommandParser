﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerCommandParser
{
    public class RequiredAttribute : Attribute { }
    public class PositionAttribute : Attribute
    {
        public int Position { get; set; }
        public PositionAttribute(int position)
        {
            Position = position;
        }
    }
    public class Parser
    {
        public static T ParseArguments<T>(string[] args, bool outputErrors = true) where T :class, new()
        {
            T result = new T();
            HashSet<string> providedArguments = new HashSet<string>();
            var properties = typeof(T).GetProperties();
            int positionArg = 0;
            List<int> positionalArguments = null;
            for(int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    var switchName = args[i].Substring(2);
                    var property = properties.SingleOrDefault(p => p.Name.Equals(switchName, StringComparison.InvariantCultureIgnoreCase));
                    if (property == null || !property.PropertyType.IsAssignableFrom(typeof(bool)))
                    {
                        if (outputErrors)
                            Console.Error.WriteLine($"No switch named {switchName} was found in the application");
                        return null;
                    }
                    property.SetValue(result, true);
                }
                else if (args[i].StartsWith("-"))
                {
                    if (i + 1 == args.Length)
                    {
                        if (outputErrors)
                            Console.Error.WriteLine("No value specified for argument {0}", args[i].Substring(1));
                        return null;
                    }
                    var paramName = args[i].Substring(1);
                    var property = properties.SingleOrDefault(p => p.Name.Equals(paramName, StringComparison.InvariantCultureIgnoreCase));
                    if (property == null)
                    {
                        if (outputErrors)
                            Console.Error.WriteLine($"No argument named {paramName} was found in the application");
                        return null;
                    }
                    property.SetValue(result, args[++i]);
                }
                else
                {

                }
            }
            return result;
        }
    }
}
