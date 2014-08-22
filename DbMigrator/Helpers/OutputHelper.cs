using System.Collections.Generic;
using System.Linq;
using DbMigrator.Helpers.Interfaces;
using System;
using System.IO;
using DbMigrator.Interfaces;

namespace DbMigrator.Helpers
{
    public class OutputHelper : IOutputHelper
    {
        public void ShowHelpOutput()
        {
            Console.WriteLine("Usage: Data.exe\r\n" +
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
                              "\t-MigrationConfig: The fully qualified class name of  DbMigrationConfiguration type.");
        }

        public void ShowInformationOutput(string connectionString, string provider, object config,
            object migrator, string targetMigration, bool script, string scriptPath)
        {
            var configType = config.GetType();
            var migratorType = migrator.GetType();

            var contextType = configType.GetProperty("ContextType").GetValue(config);
            var contextName = contextType.GetType().GetProperty("Name").GetValue(contextType);

            Console.WriteLine("Connection String: {0}\r\nProvider: {1}\r\nMigration Context: {2}",
                connectionString,
                provider,
                contextName);

            Console.WriteLine("Available Migrations: ");
            var localMigrations =
                migratorType.GetMethod("GetLocalMigrations").Invoke(migrator, new object[0]) as IEnumerable<string>;

            if (localMigrations != null)
            {
                foreach (var localMigration in localMigrations)
                {
                    Console.WriteLine("\t- {0}", localMigration);
                }
            }

            Console.WriteLine("\r\nMigrations Already Applied: ");
            var dbMigrations =
                migratorType.GetMethod("GetDatabaseMigrations").Invoke(migrator, new object[0]) as IEnumerable<string>;//migrator.GetDatabaseMigrations();

            if (dbMigrations != null)
            {
                foreach (var dbMigration in dbMigrations)
                {
                    Console.WriteLine("\t- {0}", dbMigration);
                }
            }

            Console.WriteLine("\r\nAll Pending Migrations: ");

            var pendingMigrations = migratorType.GetMethod("GetPendingMigrations").Invoke(migrator, new object[0]) as IEnumerable<string>; //migrator.GetPendingMigrations();
            if (pendingMigrations != null)
            {
                foreach (var pendingMigration in pendingMigrations)
                {
                    Console.WriteLine("\t- {0}", pendingMigration);
                }
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

        public void OutputScript(IEntityFrameworkHelper entityFrameworkHelper, object migrator, string targetMigration, string outputPath, IMessageFactory messageFactory, out IMessage message)
        {
            var scriptor = entityFrameworkHelper.GetMigratorScriptingDecoratorInstance(migrator, messageFactory, out message);
            if (message != null)
                return;

            var scriptorType = scriptor.GetType();
            var scriptParams = new object[2];
            scriptParams[0] = null;
            if (string.IsNullOrEmpty(targetMigration))
                scriptParams[1] = null;
            else
                scriptParams[1] = targetMigration;

            var sqlScript = scriptorType.GetMethod("ScriptUpdate").Invoke(scriptor, scriptParams) as string;
            if (!string.IsNullOrEmpty(outputPath))
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                File.WriteAllLines(outputPath, new[] { sqlScript });
                Console.WriteLine("Script output to: {0}", outputPath);
                return;
            }

            Console.WriteLine(sqlScript);
        }

        public int Exit(IMessage message)
        {
            var errorMessage = message.GetFormattedErrorMessage();
            if (!string.IsNullOrEmpty(errorMessage))
                Console.WriteLine(errorMessage);

            return message.Code;
        }

        public void ShowPendingMigrations(IEnumerable<string> pendingMigrations)
        {
            var migrations = pendingMigrations.ToArray();
            if (!migrations.Any())
            {
                Console.WriteLine(Messages.Messages.NoPendingMigrations);
                return;
            }

            Console.WriteLine("Pending Migrations: ");
            foreach (var migration in migrations)
            {
                Console.WriteLine("\t- {0}", migration);
            }
        }

        public void ShowTargetMigration(string targetMigration)
        {
            if (string.IsNullOrEmpty(targetMigration))
                return;

            Console.WriteLine("Target Migration: {0}", targetMigration);
        }
    }
}
