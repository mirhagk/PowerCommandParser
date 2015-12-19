using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerCommandParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnitTests.TestHelper;

namespace UnitTests
{
    [TestClass]
    public class ListTests
    {
        public class SettingsList
        {
            public enum Fruit
            {
                Apple, Orange
            }
            public List<string> StringList { get; set; }
            public List<int> IntList { get; set; }
            public Fruit[] Fruits { get; set; }
        }
        private void AssertEnumeration<T>(IEnumerable<T> list, params T[] values)
        {
            Assert.IsNotNull(list);
            Assert.AreEqual(values.Length, list.Count());
            var i = 0;
            foreach(var item in list)
            {
                Assert.AreEqual(values[i++], item);
            }
        }
        [TestMethod]
        public void StringListTest()
        {
            var settings = Parser.ParseArguments<SettingsList>(CmdLineEntryParser("-StringList a,b"));
            AssertEnumeration(settings.StringList, "a", "b");
        }
        [TestMethod]
        public void IntListTest()
        {
            var settings = Parser.ParseArguments<SettingsList>(CmdLineEntryParser("-IntList 1,2,3"));
            AssertEnumeration(settings.IntList, 1, 2, 3);
        }
        [TestMethod]
        public void ArrayOfEnumTest()
        {
            var settings = Parser.ParseArguments<SettingsList>(CmdLineEntryParser("-Fruits Apple,Orange"));
            AssertEnumeration(settings.Fruits, SettingsList.Fruit.Apple, SettingsList.Fruit.Orange);
        }
    }
}
