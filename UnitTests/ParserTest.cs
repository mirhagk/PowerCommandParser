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
            Assert.IsNull(Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("deaw deaw dq323 f42qfaw")));
            Assert.IsNull(Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("--activ ate -name mirhagk")));
            Assert.IsNull(Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("--activate -name")));
            Assert.IsNull(Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("activate --name")));
            Assert.IsNull(Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("-name \"nathan jervis")));
            Assert.IsNull(Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("-firstname --activate")));
            Assert.IsNull(Parser.ParseArguments<BasicUsageOptions>(CmdLineEntryParser("--name --activate")));
        }
        class FormatExportOptions
        {
            [Required]
            [Position(1)]
            public string Input { get; set; }
            [Required]
            [Position(2)]
            public string Output { get; set; }
            [AlternateName("f")]
            [AlternateName("form")]
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
            assert(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("input.txt -Format html output.txt")));
            assert(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("-Output output.txt input.txt")));
            assert(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("-Input input.txt output.txt")));
            assert(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("-Input input.txt -Output output.txt")));
        }
        [TestMethod]
        public void AllowsAlternateName()
        {
            var opt = Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("input.txt output.html -f html "));
            Assert.AreEqual("input.txt", opt.Input);
            Assert.AreEqual("output.html", opt.Output);
            Assert.AreEqual("html", opt.Format);
        }
        [TestMethod]
        public void AllowsMultipleAlternateNames()
        {
            var opt = Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("input.txt output.html -form html "));
            Assert.AreEqual("input.txt", opt.Input);
            Assert.AreEqual("output.html", opt.Output);
            Assert.AreEqual("html", opt.Format);
        }
        [TestMethod]
        public void EnsuresRequired()
        {
            Assert.IsNull(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("")));
            Assert.IsNull(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("input.txt")));
            Assert.IsNull(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("input.txt -Format txt")));
            Assert.IsNull(Parser.ParseArguments<FormatExportOptions>(CmdLineEntryParser("-Input input.txt")));
        }
        public class ComplexTypeSettings
        {
            public int NumberOfTrucks { get; set; }
            public long MaxSize { get; set; }
            public List<string> TruckNames { get; set; } = new List<string>();
            public bool IsAConvoy { get; set; }
            [Flags]
            public enum ConvoyType
            {
                Regular = 0,
                Great = 1,
                Big = 2,
            }
            public ConvoyType ConvoySize { get; set; } = ConvoyType.Regular;
            public bool WillPayToll { get; set; } = true;
        }
        [TestMethod]
        public void TestComplexTypeNoList()
        {
            var settings = Parser.ParseArguments<ComplexTypeSettings>(CmdLineEntryParser("-NumberOfTrucks 3 -MaxSize 10 --IsAConvoy"));
            Assert.AreEqual(3, settings.NumberOfTrucks);
            Assert.AreEqual(10, settings.MaxSize);
            Assert.AreEqual(true, settings.IsAConvoy);
            Assert.AreEqual(ComplexTypeSettings.ConvoyType.Regular, settings.ConvoySize);
            Assert.AreEqual(true, settings.WillPayToll);
            Assert.IsNotNull(settings.TruckNames);
            Assert.AreEqual(0, settings.TruckNames.Count);
        }
        [TestMethod]
        public void TestEnum()
        {
            var settings = Parser.ParseArguments<ComplexTypeSettings>(CmdLineEntryParser("-ConvoySize Regular"));
            Assert.AreEqual(ComplexTypeSettings.ConvoyType.Regular, settings.ConvoySize);
        }
        [TestMethod]
        public void TestComplexTypeAlt()
        {
            var settings = Parser.ParseArguments<ComplexTypeSettings>(CmdLineEntryParser("-NumberOfTrucks 85 -MaxSize 1000 -IsAConvoy true -ConvoySize Great,Big -WillPayToll false"));
            Assert.AreEqual(85, settings.NumberOfTrucks);
            Assert.AreEqual(1000, settings.MaxSize);
            Assert.IsTrue(settings.IsAConvoy);
            Assert.AreEqual(ComplexTypeSettings.ConvoyType.Big | ComplexTypeSettings.ConvoyType.Great, settings.ConvoySize);
            Assert.IsFalse(settings.WillPayToll);
        }
        [TestMethod]
        public void TestList()
        {
            var settings = Parser.ParseArguments<ComplexTypeSettings>(CmdLineEntryParser("-NumberOfTrucks 2 -TruckNames bob,steve"));
            Assert.IsFalse(settings.IsAConvoy);
            Assert.AreEqual(2, settings.NumberOfTrucks);
            Assert.IsNotNull(settings.TruckNames);
            Assert.AreEqual(2, settings.TruckNames.Count);
            Assert.AreEqual("bob", settings.TruckNames[0]);
            Assert.AreEqual("steve", settings.TruckNames[1]);
        }
    }
}
