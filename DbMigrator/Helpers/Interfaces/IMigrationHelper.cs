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
        Assembly LoadAssembly(out IMessage message);

        Type GetContextFromAssembly(Assembly assembly, out IMessage message);

        IEnumerable<Assembly> LoadDependencies();

        DbMigrationsConfiguration GetConfigurationInstance(Assembly assembly, Type context, string connectionString, string provider, out IMessage message);

        IMessage DoMigration(MigratorBase migrator, string targetMigration, bool showScript, string scriptPath);
    }
}
