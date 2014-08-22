namespace DbMigrator.Messages
{
    public static class Messages
    {
        public const string DllPathMissing = "Path to DLL missing";
        public const string DllNotFound = "DLL file not found";
        public const string NoContextFound = "No entities of type \"System.Data.Entity.DbContext\" found.";
        public const string ContextNameRequired = "The context name is required when more than one object of type \"System.Data.Entity.DbContext\" exist.";
        public const string ConnectionStringNotFound = "A valid connection string value could not be found.";
        public const string ConfigurationClassMissing = "Missing the DbMigration confirguation class.";
        public const string ConfigurationTypeNotFound = "Unable to find DbMigration configuration type.";
        public const string UnableToCreateInstanceOfConfirguation = "Unable to create a DbMigration instance.";
        public const string NoPendingMigrations = "No pending migrations.";
        public const string EntityFrameworkPathMissing = "Missing path to Entity Framework dll.";
        public const string CannotFindEntityFramework = "Cannot find Entity Framework dll.";
        public const string EntityFrameworkNotLoaded = "Entity Framework assembly not loaded.";
        public const string CannotFindDbMigrationsConfigurationType = "Cannot find DbMigrationsConfiguration type in Entity Framework assembly.";
        public const string CannotFindDbMigratorType = "Cannot find DbMigrator type in Entity Framework assembly.";
        public const string CannotFindDbConnectionInfoType = "Cannot find DbConnectionInfo type in Entity Framework assembly.";
        public const string UnableToCreateInstanceOfDbConnectionInfo = "Unabel to create an instance of DbConnectionInfo.";
        public const string UnableToCreateInstanceOfMigrator = "Unable to create an instance of DbMigrator.";
        public const string CannotFindScriptingDecoratorType = "Cannot find MigratorScriptingDecorator type in Entity Framework assembly.";
        public const string UnableToCreateInstanceOfScriptingDecorator = "Unable to create an instance of DbMigrator.";
        
    }
}
