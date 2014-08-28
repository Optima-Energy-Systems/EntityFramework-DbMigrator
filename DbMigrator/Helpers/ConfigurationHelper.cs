using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DbMigrator.Helpers.Interfaces;
using System.Configuration;

namespace DbMigrator.Helpers
{
    public class ConfigurationHelper : IConfigurationHelper
    {
        private const string DefaultProvider = "System.Data.SqlClient";

        public string GetConnectionString(string connectionString, string connectionStringName)
        {
            if (!string.IsNullOrEmpty(connectionString))
                return connectionString;

            if (string.IsNullOrEmpty(connectionStringName))
                return string.Empty;

            var configConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
            return configConnectionString == null ? string.Empty : configConnectionString.ConnectionString;
        }

        public string GetProvider(string provider, string connectionStringName)
        {
            if (!string.IsNullOrEmpty(provider))
                return provider;

            if (string.IsNullOrEmpty(connectionStringName))
                return DefaultProvider;

            var configConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (configConnectionString == null)
                return DefaultProvider;

            return string.IsNullOrEmpty(configConnectionString.ProviderName)
                ? DefaultProvider
                : configConnectionString.ProviderName;
        }

        public void SetAppConfig(string configPath)
        {
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                return;


            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);

            var configurationManagerType = typeof (ConfigurationManager);
            configurationManagerType.GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, 0);
            configurationManagerType.GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
            configurationManagerType.Assembly.GetTypes().First(x => x.FullName == "System.Configuration.ClientConfigPaths")
                .GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
        }
    }
}