using System.Collections.Generic;

namespace DbMigrator.Helpers.Interfaces
{
    public interface IArgumentsHelper : IDictionary<string, string>
    {
        void BuildArgumentsDictionary(string[] parameters);
        string Get(string parameter);
    }
}
