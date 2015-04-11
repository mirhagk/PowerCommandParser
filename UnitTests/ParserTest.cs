using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerCommandParser;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace UnitTests
{
    [TestClass]
    public class ParserTest
    {
        private class BasicUsageOptions
        {
            public string Name { get; set; }
            public bool Activate { get; set; }
        }
        public string[] CmdLineEntryParser(string input)
        {
            var cmdLineRegex = new Regex(@"(?:[^""\s]\S*)|(?:""[^""]*"")");
            List<string> pieces = new List<string>();
            foreach(Match match in cmdLineRegex.Matches(input))
            {
                pieces.Add(match.Value);
            }
            return pieces.ToArray();
        }
        [TestMethod]
        public void BasicUsage()
        {
            var options = Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("--activate -name mirhagk"));
            Assert.AreEqual(true, options.Activate);
            Assert.AreEqual("mirhagk", options.Name);
        }
    }
}
