using System.Configuration;
using DbMigrator.Helpers.Interfaces;
using DbMigrator.Interfaces;
using DbMigrator.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DbMigrator.Helpers
{
    public class MigrationHelper : IMigrationHelper
    {
        private const string DbContextType = "System.Data.Entity.DbContext";

        private static string GetDllPath(IArgumentsHelper argumentsHelper, IMessageFactory messageFactory, out IMessage message)
        {
            message = null;
            var path = argumentsHelper.Get(CommandLineParameters.DllPath);
            if (string.IsNullOrEmpty(path))
            {
                message = messageFactory.Get(MessageType.ParameterMissing);
                return string.Empty;
            }

            if (File.Exists(path))
                return path;

            message = messageFactory.Get(MessageType.ParameterInvalid);
            return string.Empty;
        }

        public Assembly LoadAssembly(IArgumentsHelper argumentsHelper, IMessageFactory messageFactory, out IMessage message)
        {
            try
            {
                var path = GetDllPath(argumentsHelper, messageFactory, out message);
                return message != null ? null : Assembly.LoadFile(path);
            }
            catch (FileLoadException exp)
            {
                message = messageFactory.Get(MessageType.Exception);
                message.Exception = exp;
                return null;
            }
            catch (ReflectionTypeLoadException exp)
            {
                message = messageFactory.Get(MessageType.DependenciesException);
                message.Exception = exp;
                return null;
            }
        }

        public Type GetContextFromAssembly(IArgumentsHelper argumentsHelper, IMessageFactory messageFactory, Assembly assembly, out IMessage message)
        {
            try
            {
                var contexts = assembly
                    .GetTypes()
                    .Where(
                        a =>
                            a.BaseType != null &&
                            string.Equals(a.BaseType.FullName, DbContextType,
                                StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();

                if (!contexts.Any())
                {
                    message = messageFactory.Get(MessageType.NoContextFound);
                    return null;
                }

                if (contexts.Length == 1)
                {
                    message = null;
                    return contexts.FirstOrDefault();
                }

                var contextName = argumentsHelper.Get(CommandLineParameters.ContextName);
                if (!string.IsNullOrEmpty(contextName))
                {
                    message = null;
                    return contexts
                        .FirstOrDefault(
                            x => string.Equals(x.Name, contextName, StringComparison.InvariantCultureIgnoreCase)
                                 || string.Equals(x.FullName, contextName, StringComparison.InvariantCultureIgnoreCase));
                }

                message = messageFactory.Get(MessageType.ContextNameRequired);
                return null;
            }
            catch (ReflectionTypeLoadException exp)
            {
                message = messageFactory.Get(MessageType.DependenciesException);
                message.Exception = exp;
                return null;
            }
        }

        public object GetConfigurationInstance(IArgumentsHelper argumentsHelper, IEntityFrameworkHelper entityFrameworkHelper, IMessageFactory messageFactory, Assembly assembly, Type context, string connectionString, string provider, out IMessage message)
        {
            try
            {
                var configClassName = argumentsHelper.Get(CommandLineParameters.Configuration);
                if (string.IsNullOrEmpty(configClassName))
                {
                    message = messageFactory.Get(MessageType.MissingConfigurationClass);
                    return null;
                }

                var configuration = assembly
                    .GetTypes()
                    .FirstOrDefault(x => x.FullName == configClassName);

                if (configuration == null)
                {
                    message = messageFactory.Get(MessageType.ConfigurationTypeNotFound);
                    return null;
                }

                var configConstructor = configuration.GetConstructor(Type.EmptyTypes);
                if (configConstructor == null)
                {
                    message = messageFactory.Get(MessageType.ConfigurationTypeNotFound);
                    return null;
                }

                var instance = configConstructor.Invoke(new object[0]);
                if (instance == null)
                {
                    message = messageFactory.Get(MessageType.ConfigurationTypeNotInstantiated);
                    return null;
                }

                var dbConnectionInfoInstance = entityFrameworkHelper.GetDbConnectionInfoInstance(connectionString,
                    provider, messageFactory, out message);

                if (dbConnectionInfoInstance == null)
                {
                    message = messageFactory.Get(MessageType.UnableToCreateInstanceOfDbConnectionInfo);
                    return null;
                }

                configuration.GetProperty("ContextType").SetValue(instance, context);
                configuration.GetProperty("MigrationsAssembly").SetValue(instance, assembly);
                configuration.GetProperty("TargetDatabase").SetValue(instance, dbConnectionInfoInstance);
                configuration.GetProperty("AutomaticMigrationDataLossAllowed").SetValue(instance, false);
                configuration.GetProperty("AutomaticMigrationsEnabled").SetValue(instance, true);
                
                return instance;
            }
            catch (Exception exp)
            {
                message = messageFactory.Get(MessageType.Exception);
                message.Exception = exp;
                return null;
            }
        }

        public IEnumerable<Assembly> LoadDependencies(IArgumentsHelper argumentsHelper)
        {
            var dependencies = new List<Assembly>();
            var entityFrameworkPath = argumentsHelper.Get(CommandLineParameters.EntityFramework);
            dependencies.Add(Assembly.LoadFrom(entityFrameworkPath));

            var dependentDlls = argumentsHelper.Get(CommandLineParameters.DependentDlls);
            if (string.IsNullOrEmpty(dependentDlls))
                return dependencies;

            var deps = dependentDlls.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            dependencies.AddRange(deps.Where(x => File.Exists(x.Trim()))
                .Select(x => Assembly.LoadFrom(x.Trim())));

            return dependencies;
        }

        public IMessage DoMigration(IOutputHelper outputHelper, IMessageFactory messageFactory, IEntityFrameworkHelper entityFrameworkHelper, object configuration, string targetMigration, bool showInfo, bool showScript, string scriptPath, string connectionString, string provider)
        {
            try
            {
                IMessage message;
                var migrator = entityFrameworkHelper.GetMigratorInstance(configuration, messageFactory, out message);
                if (message != null)
                    return message;

                if (migrator == null)
                    return messageFactory.Get(MessageType.UnableToCreateInstanceOfMigrator);

                var migratorType = migrator.GetType();
                var pendingMigrations =
                    migratorType.GetMethod("GetPendingMigrations").Invoke(migrator, new object[0]) as
                        IEnumerable<string>;

                outputHelper.ShowPendingMigrations(pendingMigrations);

                if (showInfo)
                {
                    outputHelper.ShowInformationOutput(connectionString, provider, configuration, migrator,
                        targetMigration, showScript, scriptPath);
                    return messageFactory.Get(MessageType.Success);
                }

                if (showScript)
                {
                    outputHelper.OutputScript(entityFrameworkHelper, migrator, targetMigration, scriptPath,
                        messageFactory, out message);
                    return message ?? messageFactory.Get(MessageType.Success);
                }

                if (!string.IsNullOrEmpty(targetMigration))
                {

                    migratorType.GetMethod("Update", new[] { typeof(string) }).Invoke(migrator, new object[] { targetMigration });
                    return messageFactory.Get(MessageType.Success);
                }

                migratorType.GetMethod("Update", Type.EmptyTypes).Invoke(migrator, new object[0]);
                return messageFactory.Get(MessageType.Success);
            }
            catch (Exception exp)
            {
                var message = messageFactory.Get(MessageType.Exception);
                message.Exception = exp;
                return message;
            }
        }
    }
}