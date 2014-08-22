using DbMigrator.Helpers;
using DbMigrator.Messages;
using DbMigrator.Tests.Utils;
using Xunit;

namespace DbMigrator.Tests.Helpers
{
    public class OutputHelperTests
    {
        [Fact]
        public void ShowTargetMigrationOutputsDetailsOfTheTargetMigration()
        {
            const string expectedOutput = "Target Migration: MockMigration";
            using (var console = new ConsoleOutputCapture())
            {
                var helper = new OutputHelper(null, null);
                helper.ShowTargetMigration("MockMigration");
                Assert.Equal(expectedOutput, console.GetOutput().Trim());
            }
        }

        [Fact]
        public void ExitOutputsTheMessageAndReturnsTheCode()
        {
            const int expectedCode = 0;
            const string expectedOutput = "ERROR 0: Mock Message";
            using (var console = new ConsoleOutputCapture())
            {
                var helper = new OutputHelper(null, null);
                var message = new Message(0, "Mock Message");
                var result = helper.Exit(message);
                Assert.Equal(expectedCode, result);
                Assert.Equal(expectedOutput, console.GetOutput().Trim());
            }
        }

        [Fact]
        public void ShowPendingMigrationsOutputsTheListOfPendingMigrations()
        {
            const string expectedOutput = "Pending Migrations: \r\n\t- MockPendingMigration";
            using (var console = new ConsoleOutputCapture())
            {
                var helper = new OutputHelper(null, null);
                helper.ShowPendingMigrations(new[] {"MockPendingMigration"});
                Assert.Equal(expectedOutput, console.GetOutput().Trim());
            }
        }

        [Fact]
        public void ShowHelpOutputShowsTheHelpText()
        {
            const string expectedOutput = "Usage: Data.exe\r\n" +
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

            using (var console = new ConsoleOutputCapture())
            {
                var helper = new OutputHelper(null, null);
                helper.ShowHelpOutput();
                Assert.Equal(expectedOutput, console.GetOutput().Trim());
            }
        }
    }
}
