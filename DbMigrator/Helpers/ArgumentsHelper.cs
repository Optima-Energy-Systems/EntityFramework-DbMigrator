using DbMigrator.Helpers.Interfaces;
using DbMigrator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbMigrator.Helpers
{
    public class ArgumentsHelper : Dictionary<string, string>, IArgumentsHelper
    {
        private readonly IFileReader _reader;

        public ArgumentsHelper() : this(new FileReader()) { }

        public ArgumentsHelper(IFileReader fileReader)
        {
            _reader = fileReader;
        }

        public void BuildArgumentsDictionary(string[] parameters)
        {
            // Allow reading the parameters from a file.
            if (parameters.Length == 1)
            {
                var split = parameters[0].Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 2 && split[0].Equals(CommandLineParameters.ConfigFile, StringComparison.InvariantCultureIgnoreCase))
                {
                    ReadArgumentsFromFile(split[1]);
                    return;
                }
            }

            foreach (var arg in parameters)
            {
                var split = arg.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (!split.Any())
                    continue;

                if (split.Length == 1)
                {
                    Add(split[0], string.Empty);
                    continue;
                }

                if (split.Length > 2)
                {
                    // this means that the second argument had '=' in it... Therefore the remaining items need to be joined back together!
                    var stringBuilder = new StringBuilder();
                    for (var i = 1; i < split.Length; i++)
                    {
                        if (i == split.Length - 1)
                        {
                            // This means we're at the last entry
                            stringBuilder.Append(split[i]);
                            continue;
                        }

                        stringBuilder.Append(string.Format("{0}=", split[i]));
                    }

                    Add(split[0], stringBuilder.ToString());
                    continue;
                }

                Add(split[0], split[1]);
            }
        }

        private void ReadArgumentsFromFile(string path)
        {
            var arguments = _reader.ReadFileLines(path);
            BuildArgumentsDictionary(arguments.ToArray());
        }

        public string Get(string parameter)
        {
            return ContainsKey(parameter) ? this[parameter] : string.Empty;
        }
    }
}
