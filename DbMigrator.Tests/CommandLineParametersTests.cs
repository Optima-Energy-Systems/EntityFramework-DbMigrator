using Xunit;

namespace DbMigrator.Tests
{
    public class CommandLineParametersTests
    {
        [Fact]
        public void DllPathCommandLineArgumentTest()
        {
            const string expectedDllPath = "-DllPath";
            const string expectedContextName = "-Context"; 
            const string expectedConnectionString = "-ConnectionString"; 
            const string expectedConnectionStringName = "-ConnectionStringName"; 
            const string expectedProvider = "-Provider"; 
            const string expectedTargetMigration = "-TargetMigration"; 
            const string expectedScript = "-Script"; 
            const string expectedScriptPath = "-ScriptPath"; 
            const string expectedInfo = "-Info"; 
            const string expectedHelp = "-Help"; 
            const string expectedDependentDlls = "-DependsOn"; 
            const string expectedAppConfigPath = "-AppConfig"; 
            const string expectedConfiguration = "-MigrationConfig"; 

            Assert.Equal(CommandLineParameters.DllPath, expectedDllPath);
            Assert.Equal(CommandLineParameters.ContextName, expectedContextName);
            Assert.Equal(CommandLineParameters.ConnectionString, expectedConnectionString);
            Assert.Equal(CommandLineParameters.ConnectionStringName, expectedConnectionStringName);
            Assert.Equal(CommandLineParameters.Provider, expectedProvider);
            Assert.Equal(CommandLineParameters.TargetMigration, expectedTargetMigration);
            Assert.Equal(CommandLineParameters.Script, expectedScript);
            Assert.Equal(CommandLineParameters.ScriptPath, expectedScriptPath);
            Assert.Equal(CommandLineParameters.Info, expectedInfo);
            Assert.Equal(CommandLineParameters.Help, expectedHelp);
            Assert.Equal(CommandLineParameters.DependentDlls, expectedDependentDlls);
            Assert.Equal(CommandLineParameters.AppConfigPath, expectedAppConfigPath);
            Assert.Equal(CommandLineParameters.Configuration, expectedConfiguration);
        }
    }
}
