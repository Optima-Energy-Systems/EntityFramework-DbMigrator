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

        Type GetContextFromAssembly(IArgumentsHelper argumentsHelper, IMessageFactory messageFactory, Assembly assembly, out IMessage message);

        IEnumerable<Assembly> LoadDependencies(IArgumentsHelper argumentsHelper);

        DbMigrationsConfiguration GetConfigurationInstance(IArgumentsHelper argumentsHelper, IMessageFactory messageFactory, Assembly assembly, Type context, string connectionString, string provider, out IMessage message);

        IMessage DoMigration(IOutputHelper outputHelper, IMessageFactory messageFactory, MigratorBase migrator, string targetMigration, bool showScript, string scriptPath);
    }
}
