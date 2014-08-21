using DbMigrator.Helpers;
using DbMigrator.Helpers.Interfaces;
using DbMigrator.Interfaces;
using DbMigrator.Messages;
using System;
using System.Linq;

namespace DbMigrator
{
    public class DbMigrator
    {
        public static int Main(string[] args)
        {
            return Main(args, new ArgumentsHelper(), new MigrationHelper(), new ConfigurationHelper(), new OutputHelper(), new MessageFactory());
        }

        public static int Main(string[] args, IArgumentsHelper argumentsHelper, IMigrationHelper migrationHelper,
            IConfigurationHelper configurationHelper, IOutputHelper outputHelper, IMessageFactory messageFactory)
        {
        IMessage error;

            // process the command line arguments
            argumentsHelper.BuildArgumentsDictionary(args);

            // get the target migration
            var targetMigration = argumentsHelper.Get(CommandLineParameters.TargetMigration);
            var scriptPath = argumentsHelper.Get(CommandLineParameters.ScriptPath);

            var showScript = argumentsHelper.ContainsKey(CommandLineParameters.Script);

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
                outputHelper.Exit(error);

            var context = migrationHelper.GetContextFromAssembly(assembly, argumentsHelper, messageFactory,
                out error);
            if (error != null)
                outputHelper.Exit(error);

            configurationHelper.SetAppConfig(argumentsHelper);

            var connectionString = configurationHelper.GetConnectionString(argumentsHelper);
            var provider = configurationHelper.GetProvider(argumentsHelper);
            if (string.IsNullOrEmpty(connectionString))
                return outputHelper.Exit(messageFactory.Get(MessageType.MissingConnectionString));

            var config = migrationHelper.GetConfigurationInstance(assembly, argumentsHelper, context,
                connectionString, provider, messageFactory, out error);
            if (error != null)
                return outputHelper.Exit(error);

            var migrator = new System.Data.Entity.Migrations.DbMigrator(config);
            if (argumentsHelper.ContainsKey(CommandLineParameters.Info))
            {
                outputHelper.ShowInformationOutput(connectionString, provider, config, migrator, targetMigration,
                    showScript, scriptPath);
                return outputHelper.Exit(messageFactory.Get(MessageType.Success));
            }

            var migrationResult = migrationHelper.DoMigration(migrator, targetMigration, showScript, scriptPath,
                outputHelper, messageFactory);

            return outputHelper.Exit(migrationResult);
        }
    }
}