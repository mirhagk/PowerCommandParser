PowerCommandParser
===

PowerCommandParser is a library that provides a way to easily parse command line arguments with the same syntax as powershell scripts.

There are several .NET libaries that provide an easy way to parse command line arguments, unfortunately most of them use unix conventions. This works for some projects, and is even an arguably better syntax, but is potentially unfamiliar to those who work with windows, as powershell provides a different syntax.

This library is meant to be a no-hassle library. Just import the `nuget` package and use the following:

```
var settings = Parser.ParseArguments<Settings>(args);
```


Installation
===

You can install this library using `nuget`

```
Install-Package PowerCommandParser
```

Use
===

To use the library first design a C# class that defines all your settings. The library is designed to work with normal classes (POCOs), so any class will do.

```
class Settings
{
	public string Input{get;set;}
	public string Output{get;set;}
	public string Format{get;set;}
}
```

Call the `Parser.ParseArguments` generic method using the class as the type argument, and passing in the program arguments (`args`)

```
var settings = Parser.ParseArguments<Settings>(args);
```

If any errors occur the method will print the errors to the standard error screen and return null. So check for null, and exit the program if it's null

```
if (settings == null) return;
```

Here's the full example:

```
using System;
using ...

namespace Example
{
	class Program
	{
		class Settings
		{
			public string Input{get;set;}
			public string Output{get;set;}
			public string Format{get;set;}
		}
		static void Main(string[] args)
		{
			var settings = Parser.ParseArguments<Settings>(args);
			if (settings == null) return;
			
			//rest of your program here
			DoWork(settings);
		}
	}
}

```

Notes
===

+ The parser only looks at public properties. If you require something different please submit an issue so it can be discussed
+ The names are case insensitive (just like powershell)
+ To use default arguments assign the default values in the constructor (or just inline).

Enums
===

Enums are supported, both flag enums and regular enums. Enums will be matched by name, and flag enums can be specified by separating them with a comma. If an enum isn't marked as required, then the default value will be 0, so using `Unspecified` as the first enum value is useful.

Switches
===

As well as parameters `-Name Value` you can also use switches `--enabled`. Switches are defined in code as booleans. Once you have a boolean you can use it as either `--enabled` or `-enabled true`/`-enabled false`

Advanced Usage
===

Besides just the very simple usage, the library also supports some additional features, such as specifying arguments as required, and using positional arguments.

Required
---

A required argument is marked with the `[Required]` attribute like so

```
class Settings
{
	[Required]
	public string Input{get;set;}
	[Required]
	public string Output{get;set;}
	public string Format{get;set;}
}
```

The library will make sure the marked fields are set and give the user errors if they aren't.

Position
---

To use positional arguments (like `myapp.exe input.txt output.txt`) use the `[Position(number)]` attribute. Arguments passed without any names will be assigned to positional arguments. E.g. `-Input myfile.txt -Output myfile.html -Format html` and `myfile.txt myfile.html -format html` and `-format html myfile.txt myfile.html` are all equivalent.

```
class Settings
{
	[Position(1)]
	[Required]
	public string Input{get;set;}
	[Position(2)]
	[Required]
	public string Output{get;set;}
	public string Format{get;set;}
}
```
