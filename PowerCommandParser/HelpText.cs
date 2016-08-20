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
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DescriptionAttribute : Attribute
    {
        public string Text { get; set; }
        public string Summary { get; set; }
		public DescriptionAttribute(){}
		public DescriptionAttribute(string summary)
		{
			Summary = summary;
		}
        public DescriptionAttribute(string summary, string text)
        {
			Summary = summary;
            Text = text;
        }
    }
	public class HelpText<T>
	{
		public string Name { get; set; }
		private DescriptionAttribute Description => typeof(T).GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
        public string ProgramName => typeof(T).Assembly.GetName().Name;
        public string Synopsis => Description?.Summary ?? ProgramName;
		public string LongDescription => Description?.Text; 
		private IEnumerable<PropertyInfo> OrderedProperties => typeof(T).GetProperties().OrderBy(p=>p.GetCustomAttributes().Min(a=>(a as PositionAttribute)?.Position) ?? int.MaxValue); 
		public string GetSyntax()
		{
			var result = Name;
			foreach (var parameter in OrderedProperties)
			{
				var positioned = parameter.GetCustomAttribute(typeof(PositionAttribute)) != null;
				var required = parameter.GetCustomAttribute(typeof(RequiredAttribute)) != null;
				var parameterType = parameter.PropertyType.ToString();
				var parameterText = parameter.Name;
				if (positioned)
					parameterText = $"[{parameterText}]";
					
				parameterText+=$" [{parameterType}]";
				if (!required)
					parameterText = $"[{parameterText}]";
					
				result += " " + parameterText;
			}
			
			return result;
		}
		
		public string GetParameterHelp()
		{
			return string.Join("\n\n", OrderedProperties
							.Select(p=>new {p,d = p.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute})
							.Where(x=>x.d!=null)
							.Select(x=>$"{x.p.Name}:\n {x.d.Summary}"));
		}
	}
}