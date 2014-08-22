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
        private readonly IMessageFactory _messageFactory;

        public EntityFrameworkHelper(IMessageFactory messageFactory)
        {
            _messageFactory = messageFactory;
        }

        public IMessage LoadEntityFramework(string entityFrameworkPath)
        {
            if (string.IsNullOrEmpty(entityFrameworkPath))
                return _messageFactory.Get(MessageType.EntityFrameworkPathMissing);

            if (!File.Exists(entityFrameworkPath))
                return _messageFactory.Get(MessageType.CannotFindEntityFramework);

            _entityFramework = Assembly.LoadFrom(entityFrameworkPath);
            return null;
        }

        public object GetMigratorInstance(object dbMigrationConfiguration, out IMessage message)
        {
            var migrator = GetTypeFromAssembly(DbMigratorType, out message);
            if (migrator == null)
                return null;

            var constr = migrator.GetConstructor(new[] { dbMigrationConfiguration.GetType() });
            return constr == null 
                ? null 
                : constr.Invoke(new[] {dbMigrationConfiguration});
        }

        public object GetDbConnectionInfoInstance(string connectionString, string provider, out IMessage message)
        {
            var dbConnectionInfoType = GetTypeFromAssembly(DbConnectionInfoType, out message);
            if (message != null)
                return null;

            if (dbConnectionInfoType == null)
            {
                message = _messageFactory.Get(MessageType.CannotFindDbConnectionInfoType);
                return null;
            }

            var constr = dbConnectionInfoType.GetConstructor(new[] { typeof(string), typeof(string) });
            if (constr != null) 
                return constr.Invoke(new object[] {connectionString, provider});

            message = _messageFactory.Get(MessageType.UnableToCreateInstanceOfDbConnectionInfo);
            return null;
        }

        public object GetMigratorScriptingDecoratorInstance(object migrator, out IMessage message)
        {
            var migratorScriptingDecorator = GetTypeFromAssembly(MigratorScriptingDecorator, out message);
            if (message != null)
                return null;

            if (migratorScriptingDecorator == null)
            {
                message = _messageFactory.Get(MessageType.CannotFindScriptingDecoratorType);
                return null;
            }

            var ctor = migratorScriptingDecorator.GetConstructor(new [] { migrator.GetType() });
            if (ctor != null) 
                return ctor.Invoke(new[] {migrator});

            message = _messageFactory.Get(MessageType.UnableToCreateInstanceOfScriptingDecorator);
            return null;
        }

        private Type GetTypeFromAssembly(string typeName, out IMessage message)
        {
            message = null;
            if (_entityFramework != null)
                return _entityFramework
                    .GetTypes()
                    .FirstOrDefault(x => x.FullName == typeName);

            message = _messageFactory.Get(MessageType.EntityFrameworkNotLoaded);
            return null;
        }
    }
}
