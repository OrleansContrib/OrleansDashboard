using System;
using OrleansDashboard.Metrics.TypeFormatting;
using TestGrains;
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
        public void TestCustomType()
        {
            var example = "ExecuteAsync(CreateApp)";

            var name = TypeFormatter.Parse(example);

            Assert.Equal("ExecuteAsync(CreateApp)", name);
        }

        [Fact]
        public void TestFriendlyNameForStrings()
        {
            var example = "TestGrains.GenericGrain`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]";

            var name = TypeFormatter.Parse(example);

            Assert.Equal("TestGrains.GenericGrain<String>", name);
        }

        [Fact]
        public void TestGenericWithMultipleTs()
        {
            var example = typeof(IGenericGrain<Tuple<string, int>>).FullName;

            var name = TypeFormatter.Parse(example);

            Assert.Equal("TestGrains.IGenericGrain<Tuple<String, Int32>>", name);
        }

        [Fact]
        public void TestGenericGrainWithMultipleTs()
        {
            var example = typeof(ITestGenericGrain<string, int>).FullName;

            var name = TypeFormatter.Parse(example);

            Assert.Equal("TestGrains.ITestGenericGrain<String, Int32>", name);
        }

        [Fact]
        public void TestGenericGrainWithFsType()
        {
            var example = ".Program.Progress";

            var name = TypeFormatter.Parse(example);

            Assert.Equal(".Program.Progress", name);
        }
    }
}