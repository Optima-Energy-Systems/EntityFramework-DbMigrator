using DbMigrator.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DbMigrator.Helpers.Interfaces
{
    public interface IMigrationHelper
    {
        Assembly LoadAssembly(string dllPath, out IMessage message);

        Type GetContextFromAssembly(string contextName, Assembly assembly, out IMessage message);

        IEnumerable<Assembly> LoadDependencies(string entityFrameworkPath, string dependentDlls);

        object GetConfigurationInstance(string configClassName, Assembly assembly, Type context, string connectionString,
            string provider, out IMessage message);

        IMessage DoMigration(object configuration, string targetMigration, bool showInfo, bool showScript,
            string scriptPath, string connectionString, string provider);
    }
}
