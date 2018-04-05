EntityFramework-DbMigrator
===

The aim of **EntityFramework-DbMigrator** is to provide a simple to use and flexible replacement for Entity Framework's migrator.exe.

## Using DbMigrator

```
DbMigrator.exe -EntityFramework={Path} -DllPath={Path}
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
	-ConfigFile={Path}
```

Details Command Line Switches:

**Required**

- **-EntityFramework**=*{Path}* - The path to the Entity Framework DLL - This is required as a command line argument to enable migrations on multiple versions of entity framework (rather than just the version referenced in the solution).
- **-DllPath**=*{Path}* - The path to the DLL containing the migrations and DbContext
- **-DependsOn**=*{Path}* - A Command seperated list of dependent DLLs that are not loaded from the GAC
- **-MigrationConfig**=*{value}* - The fully qualified name of the migration configuration class - This class must inheric from DbMigrationConfiguration _OR_ DbMigrationConfiguration<T>
- **-ConnectionString**=*{ConnectionString}* _OR_ **-ConnectionStringName**=*{Name}*
	- **-ConnectionString**=*{ConnectionString}* - The connection string
	- **-ConnectionStringName**=*{ConnectionStringName}* - The connection string to lookup in the config file

**Optional**

- **-Context**=*{ContextName}* - If more than one class derives from DbMigrationConfiguration the context name is needed to identify the context
- **-Provider**=*{ProviderName}* - The name of the database connection provider - Defaults to System.Data.SqlClient if one has not been provided as either a command line option or in the connection string
- **-TargetMigration**=*{MigrationName}* - The target migration
- **-Script** - Instead of upgrading the database just generate the SQL script
- **-ScriptPath**=*{Path}* - Path to output the generated SQL script to.
- **-Info** - Display information about what migrations have already been applied and migrations are pending
- **-Help** - Displays the usage help

## Using a Configuration File

Alternatively the arguments can be output to a file and passed to the DbMigrator. The arguments are the same as above and must be passed in one per line, however if a configuration file is to be used **ALL** arguments must be passed in via the configuration file. To invoke DbMigrator with a configuration file use:

```
DbMigrator.exe -ConfigFile={Full Path}
```

## Related Information
* [Entity Framework - Migrations 101](Wiki/Migrations_101.md)
* [Entity Framework - Migrations 201](Wiki/Migrations_201.md)

## Licence

Copyright (c) Optima Energy Systems Ltd, 1999-2018. See the [LICENSE.md file](License.md) for licence rights and
limitations (Apache 2.0).
