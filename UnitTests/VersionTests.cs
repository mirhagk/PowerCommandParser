using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class VersionTests
    {
        class SimpleArgs
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
        [TestMethod]
        public void ParsesVersion()
        {
            string output;
            var result = TestHelper.ParseArguments<SimpleArgs>("--version", out output);

            Assert.IsNull(result);
        }
        [TestMethod]
        public void StopsParsingAfterVersions()
        {
            string output;
            var result = TestHelper.ParseArguments<SimpleArgs>("-Value test --version -name", out output);

            Assert.IsNull(result);
        }
        [TestMethod]
        public void ShowsVersion()
        {
            string output;
            var result = TestHelper.ParseArguments<SimpleArgs>("-Value test --version -name", out output);

            Assert.IsTrue(output.Contains("1.0.0.0"));
            Assert.IsFalse(output.Contains("1.2.0.0"));
            Assert.IsTrue(output.Contains("UnitTests"));
        }
    }
}
