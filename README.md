EntityFramework-DbMigrator
==========================

Replacement for Entity Frameworks migrate.exe that gives better feedback. 

The aim of **EntityFramework-DbMigrator** is to provide a simple to use and flexible replacement for Entity Framework's migrator.exe.

Using DbMigrator
===

	DbMigrator.exe -DllPath={Path} 
		-DependsOn={Path1,Path2} 
		-MigrationConfig={Value} 
		-ConnectionString={ConnectionString}
		-ConnectionStringName={Name}
		-Context={ContextName}
		-Provider={Provider}
		-TargetMigration={TargetMigration}
		-Script
		-ScriptPath={Path}
		-Info
		-Help

Details Command Line Switches:

Required:

- **-DllPath**=*{Path}* - The path to the DLL containing the migrations and DbContext
- **-DependsOn**=*{Path}* - A Command seperated list of dependent DLLs that are not loaded from the GAC
- **-MigrationConfig**=*{value}* - The fully qualified name of the migration configuration class - This class must inheric from DbMigrationConfiguration _OR_ DbMigrationConfiguration<T>
- **-ConnectionString**=*{ConnectionString}* _OR_ **-ConnectionStringName**=*{Name}*
	- **-ConnectionString**=*{ConnectionString}* - The connection string
	- **-ConnectionStringName**=*{ConnectionStringName}* - The connection string to lookup in the config file

Optional: 

- **-Context**=*{ContextName}* - If more than one class derives from DbMigrationConfiguration the context name is needed to identify the context
- **-Provider**=*{ProviderName}* - The name of the database connection provider - Defaults to System.Data.SqlClient if one has not been provided as either a command line option or in the connection string
- **-TargetMigration**=*{MigrationName}* - The target migration
- **-Script** - Instead of upgrading the database just generate the SQL script
- **-ScriptPath**=*{Path}* - Path to output the generated SQL script to.
- **-Info** - Display information about what migrations have already been applied and migrations are pending
- **-Help** - Displays the usage help
