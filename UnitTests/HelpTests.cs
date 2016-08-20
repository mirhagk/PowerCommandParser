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
            var oldOut = Console.Out;
            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);
            var result = PowerCommandParser.Parser.ParseArguments<NormalNoHelp>(TestHelper.CmdLineEntryParser("--help"));
            Console.Out.Flush();
            Console.SetOut(oldOut);

            Assert.IsNull(result);
            Assert.IsFalse(stringWriter.ToString().Contains("No switch named "));
        }
    }
}
