using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestGrains;
using OrleansDashboard;
using System.ComponentModel;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class TypeFormatterTests
    {

        [TestMethod]
        public void TestSimpleType()
        {
            var example = "System.String";

            var name = TypeFormatter.Parse(example);

            Assert.AreEqual("System.String", name);
        }


        [TestMethod]
        public void TestFriendlyNameFoStrings()
        {
            var example = "TestGrains.GenericGrain`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]";

            var name = TypeFormatter.Parse(example);

            Assert.AreEqual("TestGrains.GenericGrain<String>", name);
        }

        [TestMethod]
        public void TestGenericWithMultipleTs()
        {

            var example = typeof(IGenericGrain<Tuple<string, int>>).FullName;

            Console.WriteLine(example);

            var name = TypeFormatter.Parse(example);

            Assert.AreEqual("TestGrains.IGenericGrain<Tuple<String, Int32>>", name);
        }

    }
}
