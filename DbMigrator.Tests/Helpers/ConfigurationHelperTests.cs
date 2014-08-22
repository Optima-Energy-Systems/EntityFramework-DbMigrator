using DbMigrator.Helpers;
using Xunit;

namespace DbMigrator.Tests.Helpers
{
    public class ConfigurationHelperTests
    {
        [Fact]
        public void GettingTheConnectionStringReturnsTheConnectionStringInTheArgumentsCollection()
        {
            var test = new ConfigurationHelper();
            var result = test.GetConnectionString("mockconnectionstring", string.Empty);
            Assert.Equal("mockconnectionstring", result);
        }

        [Fact]
        public void GettingTheProviderReturnsTheProviderInTheArgumentsCollection()
        {
            var test = new ConfigurationHelper();
            var result = test.GetProvider("mockprovider", string.Empty);
            Assert.Equal("mockprovider", result);
        }

        [Fact]
        public void GettingTheProviderReturnsTheDefaultProviderIfNoProviderFound()
        {
            var test = new ConfigurationHelper();
            var result = test.GetProvider(string.Empty, string.Empty);
            Assert.Equal("System.Data.SqlClient", result);
        }
    }
}
