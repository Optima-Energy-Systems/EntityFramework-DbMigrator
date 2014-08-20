using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DbMigrator
{
    public static class Helpers
    {
        private const string DefaultProvider = "System.Data.SqlClient";
        public const string DbContextType = "System.Data.Entity.DbContext";

        public static IDictionary<string, string> ProcessCommandLineArguments(IEnumerable<string> args)
        {
            var parametersDictionary = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                var split = arg.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                if (!split.Any())
                    continue;

                if (split.Length == 1)
                {
                    parametersDictionary.Add(split[0], string.Empty);
                    continue;
                }

                if (split.Length > 2)
                {
                    // this means that the second argument had '=' in it... Therefore the remaining items need to be joined back together!
                    var stringBuilder = new StringBuilder();
                    for (var i = 1; i < split.Length; i++)
                    {
                        if (i == split.Length - 1)
                        {
                            // This means we're at the last entry
                            stringBuilder.Append(split[i]);
                            continue;
                        }

                        stringBuilder.Append(string.Format("{0}=", split[i]));
                    }

                    parametersDictionary.Add(split[0], stringBuilder.ToString());
                    continue;
                }

                parametersDictionary.Add(split[0], split[1]);
            }

            return parametersDictionary;
        }

        public static string GetParameterValue(IDictionary<string, string> parameters, string parameter)
        {
            return parameters.ContainsKey(parameter) ? parameters[parameter] : string.Empty;
        }

        public static string GetHelpString()
        {
            return "Usage: Data.exe\r\n" +
                   "\t-Help\r\n" +
                   "\t-Info\r\n " +
                   "\t-DllPath\r\n" +
                   "\t-Context\r\n" +
                   "\t-ConnectionString=[ConnectionString]\r\n" +
                   "\t-ConnectionStringName=[ConnectionStringName]\r\n" +
                   "\t-Provider=[Provider]\r\n" +
                   "\t-TargetMigration=[TargetMigration]\r\n" +
                   "\t-Script\r\n" +
                   "\t-ScriptPath=[ScriptPath]\r\n" +
                   "\t-AppConfig=[AppConfig]\r\n" +
                   "\t-MigrationConfig=[MigrationConfig]\r\n\r\n" +
                   "Description of options:\r\n" +
                   "\t-Help: Displays this message\r\n" +
                   "\t-Info: When provided displays information about the state of the database and the pending migrations\r\n" +
                   "\t-DllPath: Path to the DLL containing the Database Migrations\r\n" +
                   "\t-ContextName: The Name of the DbContext derived call - Required if there is more than one context.\r\n" +
                   "\t-ConnectionString: The database connection string\r\n" +
                   "\t-ConnectionStringName: The name of the connection string in the app.config - Defaulted to: \"{0}\"\r\n" +
                   "\t-Provider: The name of the Provder - If not provided will attempt to lookup the provider in the connection string. " +
                   "If not found defaulted to: \"{1}\"\r\n" +
                   "\t-TargetMigration: The target state of the database. If the database is below this migration it will be upgraded. If the database " +
                   "is ahead of this migration the database will be downgraded.\r\n" +
                   "\t-Script: Output the generated SQL script.\r\n" +
                   "\t-ScriptPath: The path to write the generated script to.\r\n" +
                   "\t-AppConfig: The path to the app config file to use.\r\n" +
                   "\t-MigrationConfig: The fully qualified class name of  DbMigrationConfiguration type.";
        }

        public static string FormatErrorMessage(string message)
        {
            return string.Format("ERROR: {0}", message);
        }

        public static string FormatErrorMessage(Exception exp)
        {
            return string.Format("ERROR: {0}\r\n{1}\r\n{2}", exp.Message, exp.Source, exp.StackTrace);
        }

        public static Type GetContextFromAssembly(Assembly assembly, IDictionary<string, string> parameters,
            out int errorCode, out string errorMessage)
        {
            errorCode = ReturnCodes.Success;
            errorMessage = string.Empty;

            var contexts = assembly
                .GetTypes()
                .Where(x => x.BaseType != null
                            && x.BaseType.FullName == DbContextType)
                .ToArray();

            if (!contexts.Any())
            {
                errorMessage = FormatErrorMessage(Messages.NoContextFound);
                errorCode = ReturnCodes.NoContextFound;
                return null;
            }

            if (contexts.Length == 1)
                return contexts.FirstOrDefault();

            var contextName = GetParameterValue(parameters, CommandLineParameters.ContextName);
            if (!string.IsNullOrEmpty(contextName))
                return contexts
                    .FirstOrDefault(x => x.Name == parameters[CommandLineParameters.ContextName] ||
                                         x.FullName == parameters[CommandLineParameters.ContextName]);
            errorMessage = FormatErrorMessage(Messages.ContextNameRequired);
            errorCode = ReturnCodes.ContextNameRequired;
            return null;
        }

        public static IEnumerable<Assembly> LoadDependencies(IDictionary<string, string> parameters)
        {
            var dependencies = new List<Assembly>();
            var dependentDlls = GetParameterValue(parameters, CommandLineParameters.DependentDlls);
            if (string.IsNullOrEmpty(dependentDlls))
                return dependencies;

            var deps = dependentDlls.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            dependencies.AddRange(deps.Where(x => File.Exists(x.Trim()))
                .Select(x => Assembly.LoadFrom(x.Trim())));

            return dependencies;
        }

        public static string GetConnectionString(IDictionary<string, string> parameters)
        {
            var connectionString = GetParameterValue(parameters, CommandLineParameters.ConnectionString);
            if (!string.IsNullOrEmpty(connectionString))
                return parameters[CommandLineParameters.ConnectionString];

            var connectionStringName = GetParameterValue(parameters, CommandLineParameters.ConnectionStringName);
            if (string.IsNullOrEmpty(connectionStringName))
                return string.Empty;

            var configConnectionString =
                ConfigurationManager.ConnectionStrings[parameters[CommandLineParameters.ConnectionStringName]];
            return configConnectionString == null
                ? string.Empty
                : configConnectionString.ConnectionString;
        }

        public static string GetProvider(IDictionary<string, string> parameters)
        {
            // First check to see if a provider has been provided in the parameters
            var provider = GetParameterValue(parameters, CommandLineParameters.Provider);
            if (!string.IsNullOrEmpty(provider))
                return provider;

            var connectionStringName = GetParameterValue(parameters, CommandLineParameters.ConnectionStringName);
            if (string.IsNullOrEmpty(connectionStringName))
                return DefaultProvider;

            var configConnectionString =
                ConfigurationManager.ConnectionStrings[parameters[CommandLineParameters.ConnectionString]];

            if (configConnectionString == null)
                return DefaultProvider;

            return string.IsNullOrEmpty(configConnectionString.ProviderName)
                ? DefaultProvider
                : configConnectionString.ProviderName;
        }

        public static void ShowInformationOutput(string connectionString, string provider,
            DbMigrationsConfiguration config, MigratorBase migrator, string targetMigration, bool script,
            string scriptPath)
        {
            Console.WriteLine("Connection String: {0}\r\nProvider: {1}\r\nMigration Context: {2}",
                connectionString,
                provider,
                config.ContextType.Name);

            Console.WriteLine("Available Migrations: ");
            var localMigrations = migrator.GetLocalMigrations();
            foreach (var localMigration in localMigrations)
            {
                Console.WriteLine("\t- {0}", localMigration);
            }

            Console.WriteLine("\r\nMigrations Already Applied: ");
            var dbMigrations = migrator.GetDatabaseMigrations();
            foreach (var dbMigration in dbMigrations)
            {
                Console.WriteLine("\t- {0}", dbMigration);
            }

            Console.WriteLine("\r\nAll Pending Migrations: ");
            var pendingMigrations = migrator.GetPendingMigrations();
            foreach (var pendingMigration in pendingMigrations)
            {
                Console.WriteLine("\t- {0}", pendingMigration);
            }

            Console.WriteLine(string.Empty);
            if (!string.IsNullOrEmpty(targetMigration))
            {
                Console.WriteLine("Target Migration: {0}", targetMigration);
            }

            Console.WriteLine("Script Output: {0}", script ? "True" : "False");
            if (!string.IsNullOrEmpty(scriptPath))
            {
                Console.WriteLine("Script Output Path: {0}", scriptPath);
            }
        }

        public static void OutputScript(MigratorBase migrator, string target, string path)
        {
            var scriptor = new MigratorScriptingDecorator(migrator);
            var sqlScript = string.IsNullOrEmpty(target) ? scriptor.ScriptUpdate(null, null) : scriptor.ScriptUpdate(null, target);
            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                    File.Delete(path);

                File.WriteAllLines(path, new[] { sqlScript });
                Console.WriteLine("Script output to: {0}", path);
                return;
            }

            Console.WriteLine(sqlScript);
        }
    }
}
