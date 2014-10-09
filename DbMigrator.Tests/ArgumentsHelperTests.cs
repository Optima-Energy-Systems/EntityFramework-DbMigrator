using DbMigrator.Helpers;
using DbMigrator.Interfaces;
using Moq;
using Xunit;

namespace DbMigrator.Tests
{
    public class ArgumentsHelperTests
    {
        [Fact]
        public void ArgumentsHelperPopulatesDictionaryWithParameters()
        {
            var arguments = new[]
            {
                "-Arg1=Value",
                "-Arg2=AnotherValue"
            };

            var helper = new ArgumentsHelper(null);
            helper.BuildArgumentsDictionary(arguments);
            Assert.Equal(2, helper.Keys.Count);
            Assert.True(helper.ContainsKey("-Arg1"));
            Assert.Equal("Value", helper.Get("-Arg1"));
        }

        [Fact]
        public void WhenConfigFileArgumentProvidedDictionaryPopulatedFromFile()
        {
            var reader = new Mock<IFileReader>(MockBehavior.Strict);
            reader.Setup(m => m.ReadFileLines(It.IsAny<string>()))
                .Returns(new[]
                {
                    "-FileArg1=Value"
                });

            var arguments = new[]
            {
                "-ConfigFile=some/path/to/file"
            };

            var helper = new ArgumentsHelper(reader.Object);
            helper.BuildArgumentsDictionary(arguments);
            Assert.Equal(1, helper.Keys.Count);
            Assert.True(helper.ContainsKey("-FileArg1"));
            Assert.Equal("Value", helper.Get("-FileArg1"));
            reader.Verify(m => m.ReadFileLines("some/path/to/file"), Times.Once());
        }
    }
}
