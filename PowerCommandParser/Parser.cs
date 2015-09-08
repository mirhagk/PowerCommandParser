using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AlternateNameAttribute : Attribute
    {
        public string AlternateName { get; set; }
        public AlternateNameAttribute(string alternateName)
        {
            AlternateName = alternateName;
        }
    }
    public class Parser
    {
        static bool MatchesArgument(PropertyInfo property, string name)
        {
            if (property.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return true;
            foreach(AlternateNameAttribute altName in property.GetCustomAttributes(typeof(AlternateNameAttribute)))
            {
                if (altName.AlternateName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
        public static T ParseArguments<T>(string[] args, bool outputErrors = true) where T :class, new()
        {
            T result = new T();
            HashSet<string> providedArguments = new HashSet<string>();
            var properties = typeof(T).GetProperties();
            var positionalArguments =
                properties
                .Where(p => p.GetCustomAttributes(false).Any(a => a is PositionAttribute))
                .OrderBy(p => (p.GetCustomAttributes(false).Single(a => a is PositionAttribute) as PositionAttribute).Position)
                .ToList();

            var requiredArguments = properties.Where(p => p.GetCustomAttributes(false).Any(a => a is RequiredAttribute)).ToList();
            for(int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    var switchName = args[i].Substring(2);
                    var property = properties.SingleOrDefault(p => MatchesArgument(p, switchName));
                    if (property == null || !property.PropertyType.IsAssignableFrom(typeof(bool)))
                    {
                        if (outputErrors)
                            Console.Error.WriteLine($"No switch named {switchName} was found in the application");
                        return null;
                    }
                    property.SetValue(result, true);

                    providedArguments.Add(switchName);
                    positionalArguments.RemoveAll(r => r.Name == property.Name);
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
                    var property = properties.SingleOrDefault(p => MatchesArgument(p, paramName));
                    if (property == null)
                    {
                        if (outputErrors)
                            Console.Error.WriteLine($"No argument named {paramName} was found in the application");
                        return null;
                    }
                    property.SetValue(result, args[++i]);

                    providedArguments.Add(paramName);
                    positionalArguments.RemoveAll(r => r.Name == property.Name);
                }
                else
                {
                    if (!positionalArguments.Any())
                    {
                        if (outputErrors)
                            Console.Error.WriteLine($"No positional argument found to accept value {args[i]}");
                        return null;
                    }
                    var property = positionalArguments.First();
                    property.SetValue(result, args[i]);
                    positionalArguments.RemoveAt(0);

                    providedArguments.Add(property.Name);
                }
            }
            foreach(var required in requiredArguments)
            {
                if (!providedArguments.Any(a => a.Equals(required.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (outputErrors)
                        Console.Error.WriteLine($"No argument provided for required parameter ${required.Name}");
                    return null;
                }
            }
            return result;
        }
    }
}
