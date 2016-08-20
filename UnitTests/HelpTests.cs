using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class HelpTests
    {
        public class NormalNoHelp
        {
            public int Value { get; set; }
            public string Name { get; set; }
        }
        public class ClassWithHelp
        {
            [PowerCommandParser.Description("A name to use")]
            public string Name { get; set; }
        }
        [TestMethod]
        public void HelpDetected()
        {
            string output;
            var result = TestHelper.ParseArguments<NormalNoHelp>("--help", out output);

            Assert.IsNull(result);
            Assert.IsFalse(output.Contains("No switch named "));
        }
        [TestMethod]
        public void DoesntParseArguments()
        {
            string output;
            var result = TestHelper.ParseArguments<NormalNoHelp>("-Value one --help -NameSpeltWrong", out output);

            Assert.IsNull(result);
            Assert.IsFalse(output.Contains("No switch named "));
            Assert.IsFalse(output.Contains("The value specified for"));
        }
        [TestMethod]
        public void HelpDefaultsToProgramName()
        {
            string output;
            var result = TestHelper.ParseArguments<NormalNoHelp>("--help", out output);

            Assert.IsTrue(output.Contains("UnitTests"));
        }
    }
}
