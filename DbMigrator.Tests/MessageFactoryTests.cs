using DbMigrator.Messages;
using Xunit;

namespace DbMigrator.Tests
{
    public class MessageFactoryTests
    {
        private readonly MessageFactory _factory = new MessageFactory();

        [Fact]
        public void Success()
        {
            const int expectedCode = 0;
            var message = _factory.Get(MessageType.Success);
            Assert.Equal(expectedCode, message.Code);
            Assert.Equal(string.Empty, message.GetFormattedErrorMessage());
        }

        [Fact]
        public void ParameterMissing()
        {
            const int expectedCode = 1;
            var expectedMessage = string.Format("ERROR {0}: {1}", expectedCode, Messages.Messages.DllPathMissing);
            var message = _factory.Get(MessageType.ParameterMissing);
            Assert.Equal(expectedCode, message.Code);
            Assert.Equal(expectedMessage, message.GetFormattedErrorMessage());
        }

        [Fact]
        public void ParameterInvalid()
        {
            const int expectedCode = 2;
            var expectedMessage = string.Format("ERROR {0}: {1}", expectedCode, Messages.Messages.DllNotFound);
            var message = _factory.Get(MessageType.ParameterInvalid);
            Assert.Equal(expectedCode, message.Code);
            Assert.Equal(expectedMessage, message.GetFormattedErrorMessage());
        }

        [Fact]
        public void NoContextFound()
        {
            const int expectedCode = 3;
            var expectedMessage = string.Format("ERROR {0}: {1}", expectedCode, Messages.Messages.NoContextFound);
            var message = _factory.Get(MessageType.NoContextFound);
            Assert.Equal(expectedCode, message.Code);
            Assert.Equal(expectedMessage, message.GetFormattedErrorMessage());
        }

        [Fact]
        public void ContextNameRequired()
        {
            const int expectedCode = 4;
            var expectedMessage = string.Format("ERROR {0}: {1}", expectedCode, Messages.Messages.ContextNameRequired);
            var message = _factory.Get(MessageType.ContextNameRequired);
            Assert.Equal(expectedCode, message.Code);
            Assert.Equal(expectedMessage, message.GetFormattedErrorMessage());
        }

        [Fact]
        public void DependenciesException()
        {
            const int expectedCode = 5;
            var message = _factory.Get(MessageType.DependenciesException);
            Assert.Equal(expectedCode, message.Code);
        }

        [Fact]
        public void MissingConnectionString()
        {
            const int expectedCode = 6;
            var expectedMessage = string.Format("ERROR {0}: {1}", expectedCode, Messages.Messages.ConnectionStringNotFound);
            var message = _factory.Get(MessageType.MissingConnectionString);
            Assert.Equal(expectedCode, message.Code);
            Assert.Equal(expectedMessage, message.GetFormattedErrorMessage());
        }

        [Fact]
        public void MissingConfigurationClass()
        {
            const int expectedCode = 7;
            var expectedMessage = string.Format("ERROR {0}: {1}", expectedCode, Messages.Messages.ConfigurationClassMissing);
            var message = _factory.Get(MessageType.MissingConfigurationClass);
            Assert.Equal(expectedCode, message.Code);
            Assert.Equal(expectedMessage, message.GetFormattedErrorMessage());
        }

        [Fact]
        public void ConfigurationTypeNotFound()
        {
            const int expectedCode = 8;
            var expectedMessage = string.Format("ERROR {0}: {1}", expectedCode, Messages.Messages.ConfigurationTypeNotFound);
            var message = _factory.Get(MessageType.ConfigurationTypeNotFound);
            Assert.Equal(expectedCode, message.Code);
            Assert.Equal(expectedMessage, message.GetFormattedErrorMessage());
        }

        [Fact]
        public void ConfigurationTypeNotInstantiated()
        {
            const int expectedCode = 9;
            var expectedMessage = string.Format("ERROR {0}: {1}", expectedCode, Messages.Messages.UnableToCreateInstanceOfConfirguation);
            var message = _factory.Get(MessageType.ConfigurationTypeNotInstantiated);
            Assert.Equal(expectedCode, message.Code);
            Assert.Equal(expectedMessage, message.GetFormattedErrorMessage());
        }

        [Fact]
        public void Exception()
        {
            const int expectedCode = 10;
            var message = _factory.Get(MessageType.Exception);
            Assert.Equal(expectedCode, message.Code);
        }
    }
}
