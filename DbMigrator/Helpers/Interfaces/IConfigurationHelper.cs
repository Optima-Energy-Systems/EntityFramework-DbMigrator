namespace DbMigrator.Helpers.Interfaces
{
    public interface IConfigurationHelper
    {
        string GetConnectionString(string connectionString, string connectionStringName);
        string GetProvider(string provider, string connectionStringName);
        void SetAppConfig(string configPath);
    }
}
