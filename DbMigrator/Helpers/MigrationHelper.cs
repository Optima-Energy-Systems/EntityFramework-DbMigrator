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

        private static string GetDllPath(IArgumentsHelper argumentsHelper, IMessageFactory messageFactory,
            out IMessage message)
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

        public Assembly LoadAssembly(IArgumentsHelper argumentsHelper, IMessageFactory messageFactory,
            out IMessage message)
        {
            try
            {
                var path = GetDllPath(argumentsHelper, messageFactory, out message);
                return message != null ? null : Assembly.Load(path);
            }
            catch (ReflectionTypeLoadException exp)
            {
                message = messageFactory.Get(MessageType.DependenciesException);
                message.Exception = exp;
                return null;
            }
        }

        public Type GetContextFromAssembly(Assembly assembly, IArgumentsHelper argumentsHelper,
            IMessageFactory messageFactory, out IMessage message)
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
                    message = messageFactory.Get(MessageType.Success);
                    return contexts.FirstOrDefault();
                }

                var contextName = argumentsHelper.Get(CommandLineParameters.ContextName);
                if (!string.IsNullOrEmpty(contextName))
                {
                    message = messageFactory.Get(MessageType.Success);
                    return contexts
                        .FirstOrDefault(
                            x => string.Equals(x.Name, contextName, StringComparison.InvariantCultureIgnoreCase)
                                 ||
                                 string.Equals(x.FullName, contextName,
                                     StringComparison.InvariantCultureIgnoreCase));
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

        public DbMigrationsConfiguration GetConfigurationInstance(Assembly assembly, IArgumentsHelper argumentsHelper,
            Type context, string connectionString, string provider,
            IMessageFactory messageFactory, out IMessage message)
        {
            try
            {
                message = null;
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

                var instance = configConstructor.Invoke(new object[0]) as DbMigrationsConfiguration;
                if (instance == null)
                {
                    message = messageFactory.Get(MessageType.ConfigurationTypeNotInstantiated);
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
                message = messageFactory.Get(MessageType.Exception);
                message.Exception = exp;
                return null;
            }
        }

        public IEnumerable<Assembly> LoadDependencies(IArgumentsHelper argumentsHelper)
        {
            var dependencies = new List<Assembly>();
            var dependentDlls = argumentsHelper.Get(CommandLineParameters.DependentDlls);
            if (string.IsNullOrEmpty(dependentDlls))
                return dependencies;

            var deps = dependentDlls.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            dependencies.AddRange(deps.Where(x => File.Exists(x.Trim()))
                .Select(x => Assembly.LoadFrom(x.Trim())));

            return dependencies;
        }

        public IMessage DoMigration(MigratorBase migrator, string targetMigration, bool showScript, string scriptPath,
            IOutputHelper outputHelper, IMessageFactory messageFactory)
        {
            try
            {
                var pendingMigrations = migrator.GetPendingMigrations().ToArray();
                outputHelper.ShowPendingMigrations(pendingMigrations);
                if (!pendingMigrations.Any() && string.IsNullOrEmpty(targetMigration))
                    return messageFactory.Get(MessageType.Success);

                outputHelper.ShowTargetMigration(targetMigration);
                if (showScript)
                {
                    outputHelper.OutputScript(migrator, targetMigration, scriptPath);
                    return messageFactory.Get(MessageType.Success);
                }

                if (!string.IsNullOrEmpty(targetMigration))
                {
                    migrator.Update(targetMigration);
                    return messageFactory.Get(MessageType.Success);
                }

                migrator.Update();
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