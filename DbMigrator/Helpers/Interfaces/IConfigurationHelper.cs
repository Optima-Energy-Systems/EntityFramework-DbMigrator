namespace DbMigrator.Helpers.Interfaces
{
    public interface IConfigurationHelper
    {
        string GetConnectionString(IArgumentsHelper argumentsHelper);
        string GetProvider(IArgumentsHelper argumentsHelper);
        void SetAppConfig(IArgumentsHelper argumentsHelper);
    }
}
