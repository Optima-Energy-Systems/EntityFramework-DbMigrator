using System;
using DbMigrator.Messages;
using Xunit;

namespace DbMigrator.Tests
{
    public class MessageTest
    {
        [Fact]
        public void MessageReturnsTheCorrectCode()
        {
            const int expectedCode = 0;
            var message = new Message(0, string.Empty);
            Assert.Equal(expectedCode, message.Code);
        }

        [Fact]
        public void MessageReturnsTheCorrectlyFormattedMessage()
        {
            const string expectedMessage = "ERROR 1: Mocked Message";
            var message = new Message(1, "Mocked Message");
            Assert.Equal(expectedMessage, message.GetFormattedErrorMessage());
        }

        [Fact]
        public void MessageReturnsCorectlyFormattedExceptionMessageWithEmptyString()
        {
            const string expectedMessage = "ERROR 1: mocked exception\r\ntest:\r\n";
            var exp = new Exception("mocked exception")
            {
                Source = "test"
            };

            var message = new Message(1, string.Empty, exp);
            Assert.Equal(expectedMessage, message.GetFormattedErrorMessage());
        }

        [Fact]
        public void MessageReturnsCorectlyFormattedExceptionMessageWithAdditionalMessage()
        {
            const string expectedMessage = "ERROR 1: mocked message\r\nmocked exception\r\ntest:\r\n";
            var exp = new Exception("mocked exception")
            {
                Source = "test"
            };

            var message = new Message(1, "mocked message", exp);
            Assert.Equal(expectedMessage, message.GetFormattedErrorMessage());
        }
    }
}
