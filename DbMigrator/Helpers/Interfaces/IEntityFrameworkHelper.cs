using DbMigrator.Interfaces;
using System;

namespace DbMigrator.Helpers.Interfaces
{
    public interface IEntityFrameworkHelper
    {
        IMessage LoadEntityFramework(IArgumentsHelper argumentsHelper, IMessageFactory messageFactory);
        object GetMigratorInstance(object dbMigrationConfiguration, IMessageFactory messageFactory, out IMessage message);
        object GetDbConnectionInfoInstance(string connectionString, string provider, IMessageFactory messageFactory, out IMessage message);
        object GetMigratorScriptingDecoratorInstance(object migrator, IMessageFactory messageFactory, out IMessage message);
    }
}
