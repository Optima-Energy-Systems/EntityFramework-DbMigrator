namespace DbMigrator
{
    public static class Messages
    {
        public const string DllPathMissing = "Path to DLL missing";
        public const string DllNotFound = "DLL file not found";
        public const string NoContextFound = "No entities of type " + Helpers.DbContextType + " found.";
        public const string ContextNameRequired = "The context name is required when more than one object of type " + Helpers.DbContextType + " exist.";
        public const string ConnectionStringNotFound = "A valid connection string value could not be found.";
        public const string ConfigurationClassMissing = "Missing the DbMigration confirguation class.";
        public const string ConfigurationTypeNotFound = "Unable to find DbMigration configuration type.";
        public const string UnableToCreateInstanceOfConfirguation = "Unable to create a DbMigration instance.";
        public const string NoPendingMigrations = "No pending migrations.";
    }
}
