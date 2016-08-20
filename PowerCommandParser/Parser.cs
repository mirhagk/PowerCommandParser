using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        static T Cast<T>(object value)
        {
            return (T)value;
        }
        static object DynamicCast(object value,Type type)
        {
            MethodInfo castMethod = typeof(Parser).GetMethods(BindingFlags.Static|BindingFlags.NonPublic).Single(f=>f.Name==nameof(Cast)).MakeGenericMethod(type);
            return castMethod.Invoke(null, new object[] { value });
        }
        static bool ImplementsInterface(Type type, Type interfaceType)
        {
            Contract.Requires(interfaceType.IsInterface);
            if (type.GetInterfaces().Any(i => i == interfaceType))
                return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition().GetInterfaces().Any(i => i == interfaceType))
                return true;
            return false;
        }
        private static object GetObjectAsType(string value, Type type)
        {
            try
            {
                return Convert.ChangeType(value, type);
            }
            catch (Exception ex) 
            when (ex is OverflowException 
                || ex is FormatException 
                || ex is InvalidCastException)
            {
                if (type.IsEnum)
                {
                    if (value.Contains(","))
                    {
                        //Convert all the values to enums and `or` them together
                        var enumValue = value.Split(',').Select(x => (int)Enum.Parse(type, x)).Aggregate(0, (sum, x) => sum | x);
                        return DynamicCast(enumValue, type);
                    }
                    else
                        return Enum.Parse(type, value);
                }
                else if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    var values = value.Split(',');
                    var array = Array.CreateInstance(elementType, values.Length);
                    for(int i = 0; i < values.Length; i++)
                    {
                        array.SetValue(GetObjectAsType(values[i], elementType), i);
                    }
                    return array;
                }
                else if (ImplementsInterface(type, typeof(System.Collections.IList)))
                {
                    var elementType = type.GetGenericArguments()[0];

                    var list = Activator.CreateInstance(type) as System.Collections.IList;
                    foreach (var item in value.Split(','))
                    {
                        list.Add(GetObjectAsType(item, elementType));
                    }
                    return list;
                }
                else
                    throw new NotSupportedException($"{value} can't be converted to {type}");
            }
        }
        private static void SetValue(PropertyInfo property, object obj, string value)
        {
            property.SetValue(obj, GetObjectAsType(value, property.PropertyType));
        }
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

            //Check for help or version switches
            foreach(var arg in args.Select(a=>a.ToLowerInvariant()))
            {
                if (arg == "--help")
                {
                    HelpText<T> helpText = new HelpText<T>();
                    Console.WriteLine(helpText.Name);
                    Console.WriteLine(helpText.Synopsis);
                    Console.WriteLine(helpText.GetSyntax());
                    Console.WriteLine(helpText.LongDescription);
                    Console.WriteLine(helpText.GetParameterHelp());
                    return null;
                }
                if (arg == "--version")
                {
                    var assembly = typeof(T).Assembly.GetName();
                    Console.WriteLine($"{assembly.Name} {assembly.Version}");
                    return null;
                }
            }


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
                    else
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
                    try
                    {
                        SetValue(property, result, args[++i]);

                    }
                    catch (NotSupportedException ex)
                    {
                        if (outputErrors)
                            Console.Error.WriteLine(ex.Message);
                        return null;
                    }

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
