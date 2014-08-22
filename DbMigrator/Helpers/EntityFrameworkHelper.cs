using DbMigrator.Helpers.Interfaces;
using DbMigrator.Interfaces;
using DbMigrator.Messages;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DbMigrator.Helpers
{
    public class EntityFrameworkHelper : IEntityFrameworkHelper
    {
        private const string DbMigratorType = "System.Data.Entity.Migrations.DbMigrator",
            DbConnectionInfoType = "System.Data.Entity.Infrastructure.DbConnectionInfo",
            MigratorScriptingDecorator = "System.Data.Entity.Migrations.Infrastructure.MigratorScriptingDecorator";

        private Assembly _entityFramework;

        public IMessage LoadEntityFramework(IArgumentsHelper argumentsHelper, IMessageFactory messageFactory)
        {
            var path = argumentsHelper.Get(CommandLineParameters.EntityFramework);
            if (string.IsNullOrEmpty(path))
                return messageFactory.Get(MessageType.EntityFrameworkPathMissing);

            if (!File.Exists(path))
                return messageFactory.Get(MessageType.CannotFindEntityFramework);

            _entityFramework = Assembly.LoadFrom(path);
            return null;
        }

        public object GetMigratorInstance(object dbMigrationConfiguration, IMessageFactory messageFactory, out IMessage message)
        {
            var migrator = GetTypeFromAssembly(DbMigratorType, messageFactory, out message);
            if (migrator == null)
                return null;

            var constr = migrator.GetConstructor(new[] { dbMigrationConfiguration.GetType() });
            return constr == null 
                ? null 
                : constr.Invoke(new[] {dbMigrationConfiguration});
        }

        public object GetDbConnectionInfoInstance(string connectionString, string provider, IMessageFactory messageFactory, out IMessage message)
        {
            var dbConnectionInfoType = GetTypeFromAssembly(DbConnectionInfoType, messageFactory, out message);
            if (message != null)
                return null;

            if (dbConnectionInfoType == null)
            {
                message = messageFactory.Get(MessageType.CannotFindDbConnectionInfoType);
                return null;
            }

            var constr = dbConnectionInfoType.GetConstructor(new[] { typeof(string), typeof(string) });
            if (constr != null) 
                return constr.Invoke(new object[] {connectionString, provider});

            message = messageFactory.Get(MessageType.UnableToCreateInstanceOfDbConnectionInfo);
            return null;
        }

        public object GetMigratorScriptingDecoratorInstance(object migrator, IMessageFactory messageFactory, out IMessage message)
        {
            var migratorScriptingDecorator = GetTypeFromAssembly(MigratorScriptingDecorator, messageFactory, out message);
            if (message != null)
                return null;

            if (migratorScriptingDecorator == null)
            {
                message = messageFactory.Get(MessageType.CannotFindScriptingDecoratorType);
                return null;
            }

            var ctor = migratorScriptingDecorator.GetConstructor(new [] { migrator.GetType() });
            if (ctor != null) 
                return ctor.Invoke(new[] {migrator});

            message = messageFactory.Get(MessageType.UnableToCreateInstanceOfScriptingDecorator);
            return null;
        }

        private Type GetTypeFromAssembly(string typeName, IMessageFactory messageFactory, out IMessage message)
        {
            message = null;
            if (_entityFramework != null)
                return _entityFramework
                    .GetTypes()
                    .FirstOrDefault(x => x.FullName == typeName);

            message = messageFactory.Get(MessageType.EntityFrameworkNotLoaded);
            return null;
        }
    }
}
