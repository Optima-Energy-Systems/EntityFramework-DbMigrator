using DbMigrator.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace DbMigrator
{
    public class FileReader : IFileReader
    {
        public string[] ReadFileLines(string path)
        {
            var arguments = new List<string>();
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    arguments.Add(line);
                }

                reader.Close();
            }

            return arguments.ToArray();
        }
    }
}
