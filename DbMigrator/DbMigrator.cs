using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DbMigrator
{
    public class DbMigrator
    {
        private static IDictionary<string, string> _parameters;
        public static int Main(string[] args)
        {
            try
            {
                _parameters = Helpers.ProcessCommandLineArguments(args);

                var targetMigration = Helpers.GetParameterValue(_parameters, CommandLineParameters.TargetMigration);
                var scriptPath = Helpers.GetParameterValue(_parameters, CommandLineParameters.ScriptPath);

                bool showScript = _parameters.ContainsKey(CommandLineParameters.Script),
                    showInfo = _parameters.ContainsKey(CommandLineParameters.Info),
                    showHelp = _parameters.ContainsKey(CommandLineParameters.Help);

                if (showHelp)
                {
                    Console.WriteLine(Helpers.GetHelpString());
                    return ReturnCodes.Success;
                }

                var dllPath = Helpers.GetParameterValue(_parameters, CommandLineParameters.DllPath);
                if (string.IsNullOrEmpty(dllPath))
                {
                    Console.WriteLine(Helpers.FormatErrorMessage(Messages.DllPathMissing));
                    return ReturnCodes.ParameterMissing;
                }

                if (!File.Exists(dllPath))
                {
                    Console.WriteLine(Helpers.FormatErrorMessage(Messages.DllNotFound));
                    return ReturnCodes.ParameterInvalid;
                }

                var dependencies = Helpers.LoadDependencies(_parameters);
                AppDomain.CurrentDomain.AssemblyResolve +=
                    (sender, eventArgs) => dependencies.FirstOrDefault(x => x.FullName == eventArgs.Name);

                var assembly = Assembly.LoadFile(dllPath);
                int errorCode;
                string errorMessage;

                var context = Helpers.GetContextFromAssembly(assembly, _parameters, out errorCode, out errorMessage);
                if (context == null)
                {
                    Console.WriteLine(errorMessage);
                    return errorCode;
                }

                // Get the connection string
                SetAppConfig(_parameters); // Load the app.config from the specified location is necessary

                var connectionString = Helpers.GetConnectionString(_parameters);
                var provider = Helpers.GetProvider(_parameters);
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine(Helpers.FormatErrorMessage(Messages.ConnectionStringNotFound));
                    return ReturnCodes.MissingConnectionString;
                }

                var configClassName = Helpers.GetParameterValue(_parameters, CommandLineParameters.Configuration);
                if (string.IsNullOrEmpty(configClassName))
                {
                    Console.WriteLine(Helpers.FormatErrorMessage(Messages.ConfigurationClassMissing));
                    return ReturnCodes.MissingConfigurationClass;
                }

                var configuration = assembly
                    .GetTypes()
                    .FirstOrDefault(x => x.FullName == _parameters[CommandLineParameters.Configuration]);

                if (configuration == null)
                {
                    Console.WriteLine(Helpers.FormatErrorMessage(Messages.ConfigurationTypeNotFound));
                    return ReturnCodes.ConfigurationTypeNotFound;
                }

                var configConstructor = configuration.GetConstructor(Type.EmptyTypes);
                if (configConstructor == null)
                {
                    Console.WriteLine(Helpers.FormatErrorMessage(Messages.ConfigurationTypeNotFound));
                    return ReturnCodes.ConfigurationTypeNotFound;
                }

                var configInstance = configConstructor.Invoke(new object[0]) as DbMigrationsConfiguration;
                if (configInstance == null)
                {
                    Console.WriteLine(Helpers.FormatErrorMessage(Messages.UnableToCreateInstanceOfConfirguation));
                    return ReturnCodes.ConfigurationTypeNotInstantiated;
                }

                configInstance.ContextType = context;
                configInstance.MigrationsAssembly = assembly;
                configInstance.TargetDatabase = new DbConnectionInfo(connectionString, provider);
                configInstance.AutomaticMigrationDataLossAllowed = false;
                configInstance.AutomaticMigrationsEnabled = true;

                var migrator = new System.Data.Entity.Migrations.DbMigrator(configInstance);
                if (showInfo)
                {
                    Helpers.ShowInformationOutput(connectionString, provider, configInstance, migrator, targetMigration,
                        showScript, scriptPath);
                    return ReturnCodes.Success;
                }

                var pendingMigrations = migrator.GetPendingMigrations().ToArray();
                if (!pendingMigrations.Any())
                {
                    Console.WriteLine(Messages.NoPendingMigrations);
                }
                else
                {
                    Console.WriteLine("Pending Migrations: ");
                    foreach (var migration in pendingMigrations)
                    {
                        Console.WriteLine("\t {0}", migration);
                    }
                }

                if (!string.IsNullOrEmpty(targetMigration))
                {
                    Console.WriteLine("Target Migration: {0}", targetMigration);
                }

                if (showScript)
                {
                    Helpers.OutputScript(migrator, targetMigration, scriptPath);
                    return ReturnCodes.Success;
                }

                // If we have got to this point it means we want to update the database...
                if (!string.IsNullOrEmpty(targetMigration))
                {
                    migrator.Update(targetMigration);
                    return ReturnCodes.Success;
                }

                migrator.Update();
                return ReturnCodes.Success;
            }
            catch (ReflectionTypeLoadException rtle)
            {
                Console.WriteLine(Helpers.FormatErrorMessage(rtle));
                return ReturnCodes.DependenciesException;
            }
            catch (Exception exp)
            {
                Console.WriteLine(Helpers.FormatErrorMessage(exp));
                return ReturnCodes.Exception;
            }
        }

        private static void SetAppConfig(IDictionary<string, string> parameters)
        {
            if (parameters.ContainsKey(CommandLineParameters.AppConfigPath) &&
                File.Exists(parameters[CommandLineParameters.AppConfigPath]))
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", parameters[CommandLineParameters.AppConfigPath]);
            }
        }
    }
}