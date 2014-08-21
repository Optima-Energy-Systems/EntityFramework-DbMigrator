namespace DbMigrator.Helpers.Interfaces
{
    public interface IConfigurationHelper
    {
        string GetConnectionString();
        string GetProvider();
        void SetAppConfig();
    }
}
