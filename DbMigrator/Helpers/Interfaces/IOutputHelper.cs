using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using DbMigrator.Interfaces;

namespace DbMigrator.Helpers.Interfaces
{
    public interface IOutputHelper
    {
        void ShowHelpOutput();

        void ShowInformationOutput(string connectionString, string provider, DbMigrationsConfiguration config,
            MigratorBase migrator, string targetMigration, bool script, string scriptPath);

        void OutputScript(MigratorBase migrator, string target, string path);

        int Exit(IMessage message);

        void ShowPendingMigrations(IEnumerable<string> pendingMigrations);

        void ShowTargetMigration(string targetMigration);
    }
}
