namespace DbMigrator
{
    public static class CommandLineParameters
    {
        public const string EntityFramework = "-EntityFramework";
        public const string DllPath = "-DllPath";
        public const string ContextName = "-Context";
        public const string ConnectionString = "-ConnectionString";
        public const string ConnectionStringName = "-ConnectionStringName";
        public const string Provider = "-Provider";
        public const string TargetMigration = "-TargetMigration";
        public const string Script = "-Script";
        public const string ScriptPath = "-ScriptPath";
        public const string Info = "-Info";
        public const string Help = "-Help";
        public const string DependentDlls = "-DependsOn";
        public const string AppConfigPath = "-AppConfig";
        public const string Configuration = "-MigrationConfig";
    }
}
