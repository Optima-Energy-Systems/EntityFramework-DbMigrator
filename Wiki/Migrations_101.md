# Entity Framework Migrations 101

EF includes cleverness to enable management of the db schema automatically or strictly controlled via scripted commands in the Package Manager console, but unless you do stuff in the right order you're enetring a world of pain. Here's the order I found involves the least pain....

*Note: When starting work on a migration, please let others in Dev know to avoid potential merge conflicts.*

### To start with

First thing is to add the migrations feature to your project.

In the PM console... (`Alt-V, Alt-E, Alt-O`)

The PM Console shell requires a **Project Name**, from which it discovers the models and configuration details, etc., and a **Start Up Project Name** from which it discovers the db connection string. Both of these default to the current project, so your command line must specify one or other, depending which is your current folder.

If you're in Core you must specify `-StartUpProjectName Web` to help the console find the connection string.

If you're in Web you must specify `-ProjectName Core` to help the console find the models and configuration data.

Use this switch when you call the Add-Migration cmdlet:

`Add-Migrations`

This creates the ~\Migrations folder and scaffolds the initial Configuration.cs file with the settings.

### Take control

Default setting for EF is to allow automatic migrations, so that whenever you make a change to any of your models EF recognises the need for corresponding database changes and applies updates the db, so your schema stays in sync. This is all very well but you lose sight of the schema and control over data changes. Maybe this is 'control freakery' but I prefer to take charge of schema changes.

If you set `AutomaticMigrationsEnabled = false` then EF will still clock changes to the schema needed if your models change, but instead of automatically applying them just waits until you try to do something with the schema, so here's the skinny...


`Add-Migration <name> -StartUpProjectName: Web`

or

`Add-Migration <name> -ProjectName: Core`

This makes EF look at what it already knows the schema looks like (expected empty at the first run) and what it needs to look like according to defined models and relationships.

If you don't specify a name the console cmdlet will ask for one. Give it something human readable, e.g. "BaseLine" or "CreateMyNewTable" or "DropThisRedundantTable"

Scaffolding generates a partial class in a file with your given name prefixed with a date-time value.

The class includes methods for Up() and Down().

When you run

`Update-Database -StartUpProjectName: Web -verbose`

in the PM console EF checks the db to see what the last migration applied was (expected empty on the first run) and collects any later migrations (going by the prefixed datetime in the filename) in date order. For each found migration it executes the Up() method.

In this method you can create or drop tables, columns, indexes, etc., as well as execute your own SQL.

After all the pending migrations have run it executes the Seed() method (defined in ~\Core\Migrations\Configuration.cs) - I think this is a silly place for it. I suggest putting any required seed data into the individual migrations, e.g. Create table mylookups then fill mylookup with seed rows.

### Doing stuff the wrong way...

It's possible to make changes outside of this process and then retrofit a migration:

Make some schema change, like dropping a table.

At the PM prompt type

`Add-Migration`

EF examines the db, compares it with the last known version and scaffolds a change.

If you try to run this update with

`Update-Database -StartUpProjectName: Web -verbose`

EF complains that the table is already dropped, and refuses to apply the change.

Comment out all the code in the Up() method and then run the update.

EF runs the migration (with no effect on the schema because all the commands are commented out) and then logs the fact that this migration has been executed and the schema is now up to this level.

Uncomment the code steps and save the migration. If the migration needs to be run again the steps will be needed.

At the next 'Add-Migration' EF clocks that that migration has been run so won't try to rerun it, so your schema is in sync with your models.

### Also note

The PM Console is a PowerShell shell, with all the goodness that that brings, and the EF commands are fully fledged PowerShell command-lets (cmdlets) and most will accept the common switches available across many PowerShell commands, like `-help`, `-verbose` and `-whatif`, so there is always help available.

[Part Two](Migrations_201.md)

