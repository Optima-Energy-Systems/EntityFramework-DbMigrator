using DbMigrator.Helpers;
using DbMigrator.Helpers.Interfaces;
using DbMigrator.Interfaces;
using DbMigrator.Messages;
using System;
using System.Linq;

namespace DbMigrator
{
    /*
     * DbMigrator - Replacement for Migrate.exe
     * Command Line Switches: 
     * 
     * Required: 
     *  -EntityFramework={Path} - The path to the correct Entity Framework DLL - Added to enable migrations on multiple versions of entity framework
     *  -DllPath={Path} - The path to the DLL containing the migrations and DbContext
     *  -DependsOn={Path} - A Command seperated list of dependent DLLs that are not loaded from the GAC
     *  -MigrationConfig={Name} - The fully qualified name of the migration configuration class - This class must inheric from DbMigrationConfiguration OR DbMigrationConfiguration<T>
     *  -ConnectionString={ConnectionString} OR -ConnectionStringName={Name} -
     *      - ConnectionString={ConnectionString} - The connection string
     *      - ConnectionStringName={ConnectionStringName} - The connection string to lookup in the config file
     * 
     * Optional: 
     *  -Context={ContextName} - If more than one class derives from DbMigrationConfiguration the context name is needed to identify the context
     *  -Provider={ProviderName} - The name of the database connection provider - Defaults to System.Data.SqlClient if one has not been provided as either a command line option or in the connection string
     *  -TargetMigration={MigrationName} - The target migration
     *  -Script - Instead of upgrading the database just generate the SQL script
     *  -ScriptPath={Path} - Path to output the generated SQL script to.
     *  -Info - Display information about what migrations have already been applied and migrations are pending
     *  -Help - Displays the usage help
     */

    public class DbMigrator
    {
        // Create instances of the required classes.
        private static readonly IArgumentsHelper ArgumentsHelper = new ArgumentsHelper();
        private static readonly IConfigurationHelper ConfigurationHelper = new ConfigurationHelper();
        private static readonly IMessageFactory MessageFactory = new MessageFactory();
        private static readonly IEntityFrameworkHelper EntityFrameworkHelper = new EntityFrameworkHelper(MessageFactory);
        private static readonly IOutputHelper OutputHelper = new OutputHelper(EntityFrameworkHelper, MessageFactory);
        private static readonly IMigrationHelper MigrationHelper = new MigrationHelper(MessageFactory, EntityFrameworkHelper, OutputHelper);

        public static int Main(string[] args)
        {
            // process the command line arguments
            ArgumentsHelper.BuildArgumentsDictionary(args);

            var error = EntityFrameworkHelper.LoadEntityFramework(ArgumentsHelper.Get(CommandLineParameters.EntityFramework));
            if (error != null)
                return OutputHelper.Exit(error);

            // get the target migration
            var targetMigration = ArgumentsHelper.Get(CommandLineParameters.TargetMigration);
            var showScript = ArgumentsHelper.ContainsKey(CommandLineParameters.Script);
            var scriptPath = ArgumentsHelper.Get(CommandLineParameters.ScriptPath);
            
            if (ArgumentsHelper.ContainsKey(CommandLineParameters.Help))
            {
                OutputHelper.ShowHelpOutput();
                return OutputHelper.Exit(MessageFactory.Get(MessageType.Success));
            }

            var dependencies =
                MigrationHelper.LoadDependencies(ArgumentsHelper.Get(CommandLineParameters.EntityFramework),
                    ArgumentsHelper.Get(CommandLineParameters.DependentDlls));

            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, eventArgs) => dependencies.FirstOrDefault(x => x.FullName == eventArgs.Name);

            var assembly = MigrationHelper.LoadAssembly(ArgumentsHelper.Get(CommandLineParameters.DllPath), out error);
            if (error != null)
                return OutputHelper.Exit(error);

            var context = MigrationHelper.GetContextFromAssembly(ArgumentsHelper.Get(CommandLineParameters.ContextName), assembly, out error);
            if (error != null)
                return OutputHelper.Exit(error);

            ConfigurationHelper.SetAppConfig(ArgumentsHelper.Get(CommandLineParameters.AppConfigPath));

            var connectionStringName = ArgumentsHelper.Get(CommandLineParameters.ConnectionStringName);
            var connectionString = ConfigurationHelper.GetConnectionString(ArgumentsHelper.Get(CommandLineParameters.ConnectionString),
                    connectionStringName);

            var provider = ConfigurationHelper.GetProvider(ArgumentsHelper.Get(CommandLineParameters.Provider),
                connectionStringName);

            if (string.IsNullOrEmpty(connectionString))
                return OutputHelper.Exit(MessageFactory.Get(MessageType.MissingConnectionString));

            var config =
                MigrationHelper.GetConfigurationInstance(ArgumentsHelper.Get(CommandLineParameters.Configuration),
                    assembly, context, connectionString, provider, out error);
            if (error != null)
                return OutputHelper.Exit(error);

            var migrationResult = MigrationHelper.DoMigration(config, targetMigration,
                ArgumentsHelper.ContainsKey(CommandLineParameters.Info), showScript, scriptPath,
                connectionString, provider);

            return OutputHelper.Exit(migrationResult);
        }
    }
}