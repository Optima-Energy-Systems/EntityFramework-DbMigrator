using DbMigrator.Helpers;
using DbMigrator.Helpers.Interfaces;
using Moq;
using Xunit;

namespace DbMigrator.Tests.Helpers
{
    public class ConfigurationHelperTests
    {
        private class TestableHelper
        {
            public ConfigurationHelper Helper { get; private set; }
            public Mock<IArgumentsHelper> ArgumentsHelper { get; private set; }

            public TestableHelper(bool setupProvider = true)
            {
                SetupArgumentHelper(setupProvider);
                Helper = new ConfigurationHelper(ArgumentsHelper.Object);
            }

            private void SetupArgumentHelper(bool setupProvider)
            {
                ArgumentsHelper = new Mock<IArgumentsHelper>(MockBehavior.Strict);
                ArgumentsHelper.Setup(m => m.Get(CommandLineParameters.ConnectionString))
                    .Returns("mockconnectionstring");

                ArgumentsHelper.Setup(m => m.Get(CommandLineParameters.ConnectionStringName))
                    .Returns(string.Empty);

                if (setupProvider)
                {
                    ArgumentsHelper.Setup(m => m.Get(CommandLineParameters.Provider))
                        .Returns("mockprovider");
                }
                else
                {
                    ArgumentsHelper.Setup(m => m.Get(CommandLineParameters.Provider))
                        .Returns(string.Empty);
                }
            }
        }

        [Fact]
        public void GettingTheConnectionStringReturnsTheConnectionStringInTheArgumentsCollection()
        {
            var test = new TestableHelper();
            var result = test.Helper.GetConnectionString();
            Assert.Equal("mockconnectionstring", result);
            test.ArgumentsHelper.Verify(m => m.Get(CommandLineParameters.ConnectionString), Times.Once());
        }

        [Fact]
        public void GettingTheProviderReturnsTheProviderInTheArgumentsCollection()
        {
            var test = new TestableHelper();
            var result = test.Helper.GetProvider();
            Assert.Equal("mockprovider", result);
            test.ArgumentsHelper.Verify(m => m.Get(CommandLineParameters.Provider), Times.Once());
        }

        [Fact]
        public void GettingTheProviderReturnsTheDefaultProviderIfNoProviderFound()
        {
            var test = new TestableHelper(false);
            var result = test.Helper.GetProvider();
            Assert.Equal("System.Data.SqlClient", result);
            test.ArgumentsHelper.Verify(m => m.Get(CommandLineParameters.Provider), Times.Once());
        }
    }
}
