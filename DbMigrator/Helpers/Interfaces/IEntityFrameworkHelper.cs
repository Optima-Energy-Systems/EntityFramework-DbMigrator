using DbMigrator.Interfaces;

namespace DbMigrator.Helpers.Interfaces
{
    public interface IEntityFrameworkHelper
    {
        IMessage LoadEntityFramework(string entityFrameworkPath);
        object GetMigratorInstance(object dbMigrationConfiguration, out IMessage message);
        object GetDbConnectionInfoInstance(string connectionString, string provider, out IMessage message);
        object GetMigratorScriptingDecoratorInstance(object migrator, out IMessage message);
    }
}
