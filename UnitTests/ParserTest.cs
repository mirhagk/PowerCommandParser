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
            foreach (Match match in cmdLineRegex.Matches(input))
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
        [TestMethod]
        public void DoesntCrashWithInvalidFormat()
        {
            Assert.AreEqual(null, Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("deaw deaw dq323 f42qfaw")));
            Assert.AreEqual(null, Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("--activ ate -name mirhagk")));
            Assert.AreEqual(null, Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("--activate -name")));
            Assert.AreEqual(null, Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("activate --name")));
            Assert.AreEqual(null, Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("-name \"nathan jervis")));
            Assert.AreEqual(null, Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("-firstname --activate")));
            Assert.AreEqual(null, Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("--name --activate")));
        }
        class FormatExportOptions
        {
            [Required]
            [Position(1)]
            public string Input { get; set; }
            [Required]
            [Position(2)]
            public string Output { get; set; }
            public string Format { get; set; }
        }
        [TestMethod]
        public void BasicPositionalArguments()
        {
            var options = Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("input.txt output.txt"));

            Assert.AreEqual("input.txt", options.Input);
            Assert.AreEqual("output.txt", options.Output);
        }
        [TestMethod]
        public void AdvancedPositionalArguments()
        {
            Action<FormatExportOptions> assert = (opt) =>
            {
                Assert.AreEqual("input.txt", opt.Input);
                Assert.AreEqual("output.txt", opt.Output);
            };
            assert(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("input.txt -Output output.txt")));
            assert(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("-Output output.txt input.txt")));
            assert(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("-Input input.txt output.txt")));
            assert(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("-Input input.txt -Output output.txt")));
        }
    }
}
