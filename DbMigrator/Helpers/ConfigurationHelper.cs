using System;
using System.IO;
using DbMigrator.Helpers.Interfaces;
using System.Configuration;

namespace DbMigrator.Helpers
{
    public class ConfigurationHelper : IConfigurationHelper
    {
        private const string DefaultProvider = "System.Data.SqlClient";

        public string GetConnectionString(IArgumentsHelper argumentsHelper)
        {
            var connectionString = argumentsHelper.Get(CommandLineParameters.ConnectionString);
            if (!string.IsNullOrEmpty(connectionString))
                return connectionString;

            var connectionStringName = argumentsHelper.Get(CommandLineParameters.ConnectionStringName);
            if (string.IsNullOrEmpty(connectionStringName))
                return string.Empty;

            var configConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
            return configConnectionString == null ? string.Empty : configConnectionString.ConnectionString;
        }

        public string GetProvider(IArgumentsHelper argumentsHelper)
        {
            // First check to see if a provider has been provided in the parameters
            var provider = argumentsHelper.Get(CommandLineParameters.Provider);
            if (!string.IsNullOrEmpty(provider))
                return provider;

            var connectionStringName = argumentsHelper.Get(CommandLineParameters.ConnectionStringName);
            if (string.IsNullOrEmpty(connectionStringName))
                return DefaultProvider;

            var configConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (configConnectionString == null)
                return DefaultProvider;

            return string.IsNullOrEmpty(configConnectionString.ProviderName) ? DefaultProvider : configConnectionString.ProviderName;
        }

        public void SetAppConfig(IArgumentsHelper argumentsHelper)
        {
            var configPath = argumentsHelper.Get(CommandLineParameters.AppConfigPath);
            if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);
        }
    }
}