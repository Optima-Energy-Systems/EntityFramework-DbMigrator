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

        private readonly IMessageFactory _messageFactory;
        private readonly IEntityFrameworkHelper _entityFrameworkHelper;
        private readonly IOutputHelper _outputHelper;

        public MigrationHelper(IMessageFactory messageFactory, IEntityFrameworkHelper entityFrameworkHelper,
            IOutputHelper outputHelper)
        {
            _messageFactory = messageFactory;
            _entityFrameworkHelper = entityFrameworkHelper;
            _outputHelper = outputHelper;
        }

        private string GetDllPath(string dllPath, out IMessage message)
        {
            message = null;
            if (string.IsNullOrEmpty(dllPath))
            {
                message = _messageFactory.Get(MessageType.ParameterMissing);
                return string.Empty;
            }

            if (File.Exists(dllPath))
                return dllPath;

            message = _messageFactory.Get(MessageType.ParameterInvalid);
            return string.Empty;
        }

        public Assembly LoadAssembly(string dllPath, out IMessage message)
        {
            try
            {
                var path = GetDllPath(dllPath, out message);
                return message != null ? null : Assembly.LoadFile(path);
            }
            catch (FileLoadException exp)
            {
                message = _messageFactory.Get(MessageType.Exception);
                message.Exception = exp;
                return null;
            }
            catch (ReflectionTypeLoadException exp)
            {
                message = _messageFactory.Get(MessageType.DependenciesException);
                message.Exception = exp;
                return null;
            }
        }

        public Type GetContextFromAssembly(string contextName, Assembly assembly, out IMessage message)
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
                    message = _messageFactory.Get(MessageType.NoContextFound);
                    return null;
                }

                if (contexts.Length == 1)
                {
                    message = null;
                    return contexts.FirstOrDefault();
                }

                if (!string.IsNullOrEmpty(contextName))
                {
                    message = null;
                    return contexts
                        .FirstOrDefault(
                            x => string.Equals(x.Name, contextName, StringComparison.InvariantCultureIgnoreCase)
                                 || string.Equals(x.FullName, contextName, StringComparison.InvariantCultureIgnoreCase));
                }

                message = _messageFactory.Get(MessageType.ContextNameRequired);
                return null;
            }
            catch (ReflectionTypeLoadException exp)
            {
                message = _messageFactory.Get(MessageType.DependenciesException);
                message.Exception = exp;
                return null;
            }
        }

        public object GetConfigurationInstance(string configClassName, Assembly assembly, Type context,
            string connectionString, string provider, out IMessage message)
        {
            try
            {
                if (string.IsNullOrEmpty(configClassName))
                {
                    message = _messageFactory.Get(MessageType.MissingConfigurationClass);
                    return null;
                }

                var configuration = assembly
                    .GetTypes()
                    .FirstOrDefault(x => x.FullName == configClassName);

                if (configuration == null)
                {
                    message = _messageFactory.Get(MessageType.ConfigurationTypeNotFound);
                    return null;
                }

                var configConstructor = configuration.GetConstructor(Type.EmptyTypes);
                if (configConstructor == null)
                {
                    message = _messageFactory.Get(MessageType.ConfigurationTypeNotFound);
                    return null;
                }

                var instance = configConstructor.Invoke(new object[0]);
                if (instance == null)
                {
                    message = _messageFactory.Get(MessageType.ConfigurationTypeNotInstantiated);
                    return null;
                }

                var dbConnectionInfoInstance = _entityFrameworkHelper.GetDbConnectionInfoInstance(connectionString,
                    provider, out message);

                if (dbConnectionInfoInstance == null)
                {
                    message = _messageFactory.Get(MessageType.UnableToCreateInstanceOfDbConnectionInfo);
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
                message = _messageFactory.Get(MessageType.Exception);
                message.Exception = exp;
                return null;
            }
        }

        public IEnumerable<Assembly> LoadDependencies(string entityFrameworkPath, string dependentDlls)
        {
            //var entityFrameworkPath = argumentsHelper.Get(CommandLineParameters.EntityFramework);
            //var dependentDlls = argumentsHelper.Get(CommandLineParameters.DependentDlls);

            var dependencies = new List<Assembly>
            {
                Assembly.LoadFrom(entityFrameworkPath)
            };

            if (string.IsNullOrEmpty(dependentDlls))
                return dependencies;

            var deps = dependentDlls.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            dependencies.AddRange(deps.Where(x => File.Exists(x.Trim()))
                .Select(x => Assembly.LoadFrom(x.Trim())));

            return dependencies;
        }

        public IMessage DoMigration(object configuration, string targetMigration, bool showInfo, bool showScript,
            string scriptPath, string connectionString, string provider)
        {
            try
            {
                IMessage message;
                var migrator = _entityFrameworkHelper.GetMigratorInstance(configuration, out message);
                if (message != null)
                    return message;

                if (migrator == null)
                    return _messageFactory.Get(MessageType.UnableToCreateInstanceOfMigrator);

                var migratorType = migrator.GetType();
                var pendingMigrations =
                    migratorType.GetMethod("GetPendingMigrations").Invoke(migrator, new object[0]) as
                        IEnumerable<string>;

                _outputHelper.ShowPendingMigrations(pendingMigrations);

                if (showInfo)
                {
                    _outputHelper.ShowInformationOutput(connectionString, provider, configuration, migrator,
                        targetMigration, showScript, scriptPath);
                    return _messageFactory.Get(MessageType.Success);
                }

                if (showScript)
                {
                    _outputHelper.OutputScript(migrator, targetMigration, scriptPath, out message);
                    return message ?? _messageFactory.Get(MessageType.Success);
                }

                if (!string.IsNullOrEmpty(targetMigration))
                {

                    migratorType.GetMethod("Update", new[] {typeof (string)})
                        .Invoke(migrator, new object[] {targetMigration});
                    return _messageFactory.Get(MessageType.Success);
                }

                migratorType.GetMethod("Update", Type.EmptyTypes).Invoke(migrator, new object[0]);
                return _messageFactory.Get(MessageType.Success);
            }
            catch (Exception exp)
            {
                var message = _messageFactory.Get(MessageType.Exception);
                message.Exception = exp;
                return message;
            }
        }
    }
}