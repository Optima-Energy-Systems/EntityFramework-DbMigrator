using System.Collections.Generic;
using DbMigrator.Messages;

namespace DbMigrator.Interfaces
{
    public interface IMessageFactory : IDictionary<MessageType, IMessage>
    {
        IMessage Get(MessageType type);
    }
}
