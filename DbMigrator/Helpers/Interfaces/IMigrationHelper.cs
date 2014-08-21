using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using DbMigrator.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DbMigrator.Helpers.Interfaces
{
    public interface IMigrationHelper
    {
        Assembly LoadAssembly(IArgumentsHelper argumentsHelper, IMessageFactory messageFactory, out IMessage message);

        Type GetContextFromAssembly(Assembly assembly, IArgumentsHelper argumentsHelper, IMessageFactory messageFactory,
            out IMessage message);

        IEnumerable<Assembly> LoadDependencies(IArgumentsHelper argumentsHelper);

        DbMigrationsConfiguration GetConfigurationInstance(Assembly assembly, IArgumentsHelper argumentsHelper,
            Type context, string connectionString, string provider, IMessageFactory messageFactory, out IMessage message);

        IMessage DoMigration(MigratorBase migrator, string targetMigration, bool showScript, string scriptPath,
            IOutputHelper outputHelper, IMessageFactory messageFactory);
    }
}
