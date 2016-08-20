using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerCommandParser;
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
        [PowerCommandParser.Description("Sample usage of help attributes")]
        public class ClassWithHelp
        {
            [PowerCommandParser.Description("A name to use")]
            public string Name { get; set; }
            public int Value { get; set; }
            [Required]
            public string Required { get; set; }
            [Position(1)]
            public int Positional { get; set; }
            [Position(2)]
            [Required]
            public float PositionalAndRequired { get; set; }
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
        [TestMethod]
        public void SynopsisIsUsed()
        {
            string output;
            var result = TestHelper.ParseArguments<ClassWithHelp>("--help", out output);

            Assert.IsTrue(output.Contains("Sample usage of help attributes"));
            Assert.IsFalse(output.Contains("UnitTests"));
        }
        [TestMethod]
        public void ParamDetailsUsed()
        {
            string output;
            var result = TestHelper.ParseArguments<ClassWithHelp>("--help", out output);

            Assert.IsTrue(output.Contains("A name to use"));
        }
        [TestMethod]
        public void SyntaxIsShown()
        {
            string output;
            var result = TestHelper.ParseArguments<ClassWithHelp>("--help", out output);
            
            Assert.IsTrue(output.Contains("[Name System.String]"));
            Assert.IsTrue(output.Contains("[Value System.Int32]"));
            Assert.IsTrue(output.Contains("Required System.String"));
            Assert.IsFalse(output.Contains("[Required System.String]"));
            Assert.IsTrue(output.Contains("[[Positional] System.Int32]"));
            Assert.IsTrue(output.Contains("[PositionalAndRequired] System.Single"));
            Assert.IsFalse(output.Contains("[[PositionalAndRequired] System.Single]"));
            Assert.IsTrue(output.IndexOf("[[Positional] System.Int32]") < output.IndexOf("[PositionalAndRequired] System.Single"));
        }
    }
}
