using DbMigrator.Helpers.Interfaces;
using DbMigrator.Interfaces;
using DbMigrator.Messages;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DbMigrator.Helpers
{
    public class MigrationHelper : IMigrationHelper
    {
        private const string DbContextType = "System.Data.Entity.DbContext";

        private readonly IArgumentsHelper _argumentsHelper;
        private readonly IMessageFactory _messageFactory;
        private readonly IOutputHelper _outputHelper;

        public MigrationHelper() : this(new ArgumentsHelper(), new MessageFactory(), new OutputHelper()) { }

        public MigrationHelper(IArgumentsHelper argumentsHelper, IMessageFactory messageFactory, IOutputHelper outputHelper)
        {
            _argumentsHelper = argumentsHelper;
            _messageFactory = messageFactory;
            _outputHelper = outputHelper;
        }

        private string GetDllPath(out IMessage message)
        {
            message = null;
            var path = _argumentsHelper.Get(CommandLineParameters.DllPath);
            if (string.IsNullOrEmpty(path))
            {
                message = _messageFactory.Get(MessageType.ParameterMissing);
                return string.Empty;
            }

            if (File.Exists(path))
                return path;

            message = _messageFactory.Get(MessageType.ParameterInvalid);
            return string.Empty;
        }

        public Assembly LoadAssembly(out IMessage message)
        {
            try
            {
                var path = GetDllPath(out message);
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

        public Type GetContextFromAssembly(Assembly assembly, out IMessage message)
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

                var contextName = _argumentsHelper.Get(CommandLineParameters.ContextName);
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

        public DbMigrationsConfiguration GetConfigurationInstance(Assembly assembly, Type context, string connectionString, string provider, out IMessage message)
        {
            try
            {
                message = null;
                var configClassName = _argumentsHelper.Get(CommandLineParameters.Configuration);
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

                var instance = configConstructor.Invoke(new object[0]) as DbMigrationsConfiguration;
                if (instance == null)
                {
                    message = _messageFactory.Get(MessageType.ConfigurationTypeNotInstantiated);
                    return null;
                }

                instance.ContextType = context;
                instance.MigrationsAssembly = assembly;
                instance.TargetDatabase = new DbConnectionInfo(connectionString, provider);
                instance.AutomaticMigrationDataLossAllowed = false;
                instance.AutomaticMigrationsEnabled = true;

                return instance;
            }
            catch (Exception exp)
            {
                message = _messageFactory.Get(MessageType.Exception);
                message.Exception = exp;
                return null;
            }
        }

        public IEnumerable<Assembly> LoadDependencies()
        {
            var dependencies = new List<Assembly>();
            var dependentDlls = _argumentsHelper.Get(CommandLineParameters.DependentDlls);
            if (string.IsNullOrEmpty(dependentDlls))
                return dependencies;

            var deps = dependentDlls.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            dependencies.AddRange(deps.Where(x => File.Exists(x.Trim()))
                .Select(x => Assembly.LoadFrom(x.Trim())));

            return dependencies;
        }

        public IMessage DoMigration(MigratorBase migrator, string targetMigration, bool showScript, string scriptPath)
        {
            try
            {
                var pendingMigrations = migrator.GetPendingMigrations().ToArray();
                _outputHelper.ShowPendingMigrations(pendingMigrations);
                if (!pendingMigrations.Any() && string.IsNullOrEmpty(targetMigration))
                    return _messageFactory.Get(MessageType.Success);

                _outputHelper.ShowTargetMigration(targetMigration);
                if (showScript)
                {
                    _outputHelper.OutputScript(migrator, targetMigration, scriptPath);
                    return _messageFactory.Get(MessageType.Success);
                }

                if (!string.IsNullOrEmpty(targetMigration))
                {
                    migrator.Update(targetMigration);
                    return _messageFactory.Get(MessageType.Success);
                }

                migrator.Update();
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