using System;
using DbMigrator.Interfaces;

namespace DbMigrator.Messages
{
    public class Message : IMessage
    {
        public int Code { get; private set; }
        public Exception Exception { get; set; }
        private readonly string _message;

        public Message(int code, string message, Exception exception = null)
        {
            Code = code;
            _message = message;
            if (exception != null)
                Exception = exception;
        }

        public string GetFormattedErrorMessage()
        {
            if (Exception != null)
                return GetFormattedErrorMessage(Exception);

            return string.IsNullOrEmpty(_message) 
                ? string.Empty 
                : string.Format("ERROR {0}: {1}", Code, _message);
        }

        private string GetFormattedErrorMessage(Exception exp)
        {
            if (!string.IsNullOrEmpty(_message))
                return string.Format("ERROR {0}: {1}\r\n{2}\r\n{3}:\r\n{4}",
                    Code,
                    _message,
                    exp.Message,
                    exp.Source,
                    exp.StackTrace);

            return string.Format("ERROR {0}: {1}\r\n{2}:\r\n{3}",
                Code,
                exp.Message,
                exp.Source,
                exp.StackTrace);
        }
    }
}
