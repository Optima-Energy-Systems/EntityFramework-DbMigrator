using DbMigrator.Helpers;
using Xunit;

namespace DbMigrator.Tests.Helpers
{
    public class ArgumentsHelperTests
    {
        [Fact]
        public void ArgumentsHelplerCorrectlyParsesTheArgumentsArray()
        {
            const int expectedKeys = 2;
            var helper = CreateArgumentsHelper();
            Assert.Equal(expectedKeys, helper.Keys.Count);
            Assert.True(helper.ContainsKey("-Arg1"));
            Assert.True(helper.ContainsKey("-Arg2"));
        }

        [Fact]
        public void ArgumentsHelperGetsValuesFromWithTheDictionary()
        {
            const string expectedValue = "Value";
            var helper = CreateArgumentsHelper();
            Assert.True(helper.ContainsKey("-Arg1"));
            Assert.Equal(expectedValue, helper.Get("-Arg1"));
        }

        private ArgumentsHelper CreateArgumentsHelper()
        {
            var helper = new ArgumentsHelper();
            helper.BuildArgumentsDictionary(new[] {"-Arg1=Value", "-Arg2=Value1"});
            return helper;
        }
    }
}
