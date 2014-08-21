using System;
using System.IO;
using DbMigrator.Helpers.Interfaces;
using System.Configuration;

namespace DbMigrator.Helpers
{
    public class ConfigurationHelper : IConfigurationHelper
    {
        private const string DefaultProvider = "System.Data.SqlClient";

        private readonly IArgumentsHelper _argumentsHelper;

        public ConfigurationHelper() : this(new ArgumentsHelper()) { }

        public ConfigurationHelper(IArgumentsHelper argumentsHelper)
        {
            _argumentsHelper = argumentsHelper;
        }

        public string GetConnectionString()
        {
            var connectionString = _argumentsHelper.Get(CommandLineParameters.ConnectionString);
            if (!string.IsNullOrEmpty(connectionString))
                return connectionString;

            var connectionStringName = _argumentsHelper.Get(CommandLineParameters.ConnectionStringName);
            if (string.IsNullOrEmpty(connectionStringName))
                return string.Empty;

            var configConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
            return configConnectionString == null ? string.Empty : configConnectionString.ConnectionString;
        }

        public string GetProvider()
        {
            // First check to see if a provider has been provided in the parameters
            var provider = _argumentsHelper.Get(CommandLineParameters.Provider);
            if (!string.IsNullOrEmpty(provider))
                return provider;

            var connectionStringName = _argumentsHelper.Get(CommandLineParameters.ConnectionStringName);
            if (string.IsNullOrEmpty(connectionStringName))
                return DefaultProvider;

            var configConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (configConnectionString == null)
                return DefaultProvider;

            return string.IsNullOrEmpty(configConnectionString.ProviderName) ? DefaultProvider : configConnectionString.ProviderName;
        }

        public void SetAppConfig()
        {
            var configPath = _argumentsHelper.Get(CommandLineParameters.AppConfigPath);
            if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);
        }
    }
}