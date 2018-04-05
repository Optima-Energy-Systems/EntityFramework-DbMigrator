# Migrations for >1 devs

Check out my [Migrations 101](Migrations_101.md) for basic info about EF migrations.

That document starts you off but when you're working in a multi-developer environment, with source control on your code, you may find the following scenario troublesome:

The db schema evolves through 42 revisions. **__MigrationHistory** records all the changes, and there are 42 corresponding migration code files in the core project.

The repo has a **master** branch with 17501 change sets and everything is up to date.

All is sweet.

On Monday morning 'Adam' begins work on a new feature and creates a branch (or fork).

On Monday afternoon 'Bob' begins work on an unrelated feature and creates his own branch (or fork).

On Tuesday morning Adam needs a new model and corresponding table, so he creates a migration and updates the db schema. Db is at revision 43.

On Tuesday afternoon Bob needs a new model and corresponding table, so he also creates a migration. Bobs migration doesn't have code for Adams model, but the db schema includes Adams table, so Bob's migration Up() method starts by dropping Adams table to make the db schema match Bob's view of the codebase. Bob could simply delete the offending lines of code to ensure that Adam's table is not dropped but the BLOb stored along with the migration identifier in **__MigrationHistory** no longer reflects the truth of the db schema. Bob has more work to do and exects to have to create more tables, so doesn't apply his db update.

On Wednesday morning Adam needs another model and table. He creates a migration unhindered, because the latest migration BLOb matches up with the models in his codebase. Adam completes his work and applies his update. The db schema is now at revision 44.

On Wednesday afternoon Bob has finished his model and is ready to add another migration. Again Bob's migration suggests dropping Adams tables because Bob's doesn't have corresponding models. Bob might just delete the offending lines.

Bob finishes his work and updates the database. EF checks the current level and finds The db up to Wednesday morning, so tries to apply Bob's Wednesday afternoon migration. ***Whoa!*** What happened to Bob's Tuesday afternoon migration? EF disregarded it because it only tries to apply migrations ***later*** than the last one applied.

### Are we screwed?

**No**

We need a policy or process defined by which we can ensure that the master branch of the codebase is kept in sync with the db schema.

So, as soon as Adam decides that he needs a new model, he should:

1. Create and switch to a new branch for the models and migration only.
1. Define his model(s) in code along with any necessary EF configurations, etc.
1. If existing tables are to be changed then models must be arranged such that data may be omitted without breaking anything.
1. Create a migration.
1. Apply the migration to the developers' shared db.
1. Push the changes to source control.
1. Merge the changes with the master branch.
1. Discard the branch.

In this way the db schema and master branch stay in sync and migrations are applied in the correct sequence regardless of anyone's unfinished work.

If relationships must be defined it may be necessary to create null-able properties so that normal operations can continue while further work is undertaken. A further migration can be applied (using the process described above) to alter properties later.

The key thing is the need to keep the master branch's codebase in sync with the db schema, and to make sure that the sequential nature of the migrations is maintained.
