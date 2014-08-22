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
        public static int Main(string[] args)
        {
            return Main(args, new ArgumentsHelper(), new EntityFrameworkHelper(), new MigrationHelper(), new ConfigurationHelper(), new OutputHelper(), new MessageFactory());
        }

        public static int Main(string[] args, IArgumentsHelper argumentsHelper, IEntityFrameworkHelper entityFrameworkHelper, IMigrationHelper migrationHelper,
            IConfigurationHelper configurationHelper, IOutputHelper outputHelper, IMessageFactory messageFactory)
        {
            IMessage error;

            // process the command line arguments
            argumentsHelper.BuildArgumentsDictionary(args);

            error = entityFrameworkHelper.LoadEntityFramework(argumentsHelper, messageFactory);
            if (error != null)
                return outputHelper.Exit(error);

            // get the target migration
            var targetMigration = argumentsHelper.Get(CommandLineParameters.TargetMigration);
            var showScript = argumentsHelper.ContainsKey(CommandLineParameters.Script);
            var scriptPath = argumentsHelper.Get(CommandLineParameters.ScriptPath);
            
            if (argumentsHelper.ContainsKey(CommandLineParameters.Help))
            {
                outputHelper.ShowHelpOutput();
                return outputHelper.Exit(messageFactory.Get(MessageType.Success));
            }

            var dependencies = migrationHelper.LoadDependencies(argumentsHelper);
            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, eventArgs) => dependencies.FirstOrDefault(x => x.FullName == eventArgs.Name);

            var assembly = migrationHelper.LoadAssembly(argumentsHelper, messageFactory, out error);
            if (error != null)
                return outputHelper.Exit(error);

            var context = migrationHelper.GetContextFromAssembly(argumentsHelper, messageFactory, assembly, out error);
            if (error != null)
                return outputHelper.Exit(error);

            configurationHelper.SetAppConfig(argumentsHelper);

            var connectionString = configurationHelper.GetConnectionString(argumentsHelper);
            var provider = configurationHelper.GetProvider(argumentsHelper);
            if (string.IsNullOrEmpty(connectionString))
                return outputHelper.Exit(messageFactory.Get(MessageType.MissingConnectionString));

            var config = migrationHelper.GetConfigurationInstance(argumentsHelper, entityFrameworkHelper, messageFactory, assembly, context, connectionString, provider, out error);
            if (error != null)
                return outputHelper.Exit(error);

            var migrationResult = migrationHelper.DoMigration(outputHelper, messageFactory, entityFrameworkHelper,
                config, targetMigration, argumentsHelper.ContainsKey(CommandLineParameters.Info), showScript, scriptPath,
                connectionString, provider);

            return outputHelper.Exit(migrationResult);
        }
    }
}