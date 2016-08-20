using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UnitTests
{
    class TestHelper
    {
        public static string[] CmdLineEntryParser(string input)
        {
            var cmdLineRegex = new Regex(@"(?:[^""\s]\S*)|(?:""[^""]*"")");
            List<string> pieces = new List<string>();
            foreach (Match match in cmdLineRegex.Matches(input))
            {
                pieces.Add(match.Value);
            }
            return pieces.ToArray();
        }
        public static T ParseArguments<T>(string input, out string output) where T : class, new()
        {
            var oldOut = Console.Out;
            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);
            var result = PowerCommandParser.Parser.ParseArguments<T>(TestHelper.CmdLineEntryParser(input));
            Console.Out.Flush();
            Console.SetOut(oldOut);
            output = stringWriter.ToString();

            return result;
        }
    }
}
