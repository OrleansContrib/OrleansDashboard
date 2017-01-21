using System;
using TestGrains;
using OrleansDashboard;
using System.ComponentModel;
using System.Linq;
using Xunit;

namespace UnitTests
{
    public class TypeFormatterTests
    {

        [Fact]
        public void TestSimpleType()
        {
            var example = "System.String";

            var name = TypeFormatter.Parse(example);

            Assert.Equal("System.String", name);
        }


        [Fact]
        public void TestFriendlyNameFoStrings()
        {
            var example = "TestGrains.GenericGrain`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]";

            var name = TypeFormatter.Parse(example);

            Assert.Equal("TestGrains.GenericGrain<String>", name);
        }

        [Fact]
        public void TestGenericWithMultipleTs()
        {

            var example = typeof(IGenericGrain<Tuple<string, int>>).FullName;

            Console.WriteLine(example);

            var name = TypeFormatter.Parse(example);

            Assert.Equal("TestGrains.IGenericGrain<Tuple<String, Int32>>", name);
        }

    }
}
