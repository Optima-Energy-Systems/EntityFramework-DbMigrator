using System.Collections.Generic;
using DbMigrator.Interfaces;

namespace DbMigrator.Messages
{
    public class MessageFactory : Dictionary<MessageType, IMessage>, IMessageFactory
    {
        public MessageFactory()
        {
            BuildFactory();
        }

        private void BuildFactory()
        {
            Add(MessageType.Success, new Message((int)MessageType.Success, string.Empty));
            Add(MessageType.ParameterMissing, new Message((int)MessageType.ParameterMissing, Messages.DllPathMissing));
            Add(MessageType.ParameterInvalid, new Message((int)MessageType.ParameterInvalid, Messages.DllNotFound));
            Add(MessageType.NoContextFound, new Message((int)MessageType.NoContextFound, Messages.NoContextFound));
            Add(MessageType.ContextNameRequired, new Message((int)MessageType.ContextNameRequired, Messages.ContextNameRequired));
            Add(MessageType.DependenciesException, new Message((int)MessageType.DependenciesException, string.Empty)); // Special exception case
            Add(MessageType.MissingConnectionString, new Message((int)MessageType.MissingConnectionString, Messages.ConnectionStringNotFound));
            Add(MessageType.MissingConfigurationClass, new Message((int)MessageType.MissingConfigurationClass, Messages.ConfigurationClassMissing));
            Add(MessageType.ConfigurationTypeNotFound, new Message((int)MessageType.ConfigurationTypeNotFound, Messages.ConfigurationTypeNotFound));
            Add(MessageType.ConfigurationTypeNotInstantiated, new Message((int)MessageType.ConfigurationTypeNotInstantiated, Messages.UnableToCreateInstanceOfConfirguation));
            Add(MessageType.Exception, new Message((int)MessageType.Exception, string.Empty)); // Special exception case
        }

        public IMessage Get(MessageType type)
        {
            return ContainsKey(type) ? this[type] : null;
        }
    }
}
