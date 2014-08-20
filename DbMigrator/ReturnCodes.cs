namespace DbMigrator
{
    public static class ReturnCodes
    {
        public const int Success = 0;
        public const int ParameterMissing = 1;
        public const int ParameterInvalid = 2;
        public const int NoContextFound = 3;
        public const int ContextNameRequired = 4;
        public const int DependenciesException = 5;
        public const int MissingConnectionString = 6;
        public const int MissingConfigurationClass = 7;
        public const int ConfigurationTypeNotFound = 8;
        public const int ConfigurationTypeNotInstantiated = 9;
        public const int Exception = 10;
    }
}
