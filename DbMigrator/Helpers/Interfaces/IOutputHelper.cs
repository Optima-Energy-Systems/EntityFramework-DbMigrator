using System.Collections.Generic;
using DbMigrator.Interfaces;

namespace DbMigrator.Helpers.Interfaces
{
    public interface IOutputHelper
    {
        void ShowHelpOutput();

        void ShowInformationOutput(string connectionString, string provider, object config,
            object migrator, string targetMigration, bool script, string scriptPath);
        
        void OutputScript(object migrator, string target, string path, out IMessage message);

        int Exit(IMessage message);

        void ShowPendingMigrations(IEnumerable<string> pendingMigrations);

        void ShowTargetMigration(string targetMigration);
    }
}
