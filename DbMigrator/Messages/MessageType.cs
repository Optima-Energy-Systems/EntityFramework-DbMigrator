namespace DbMigrator.Messages
{
    public enum MessageType
    {
        Success = 0,
        ParameterMissing = 1,
        ParameterInvalid = 2,
        NoContextFound = 3,
        ContextNameRequired = 4,
        DependenciesException = 5,
        MissingConnectionString = 6,
        MissingConfigurationClass = 7,
        ConfigurationTypeNotFound = 8,
        ConfigurationTypeNotInstantiated = 9,
        Exception = 10
    }
}
