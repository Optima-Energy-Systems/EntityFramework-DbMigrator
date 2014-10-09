namespace DbMigrator.Interfaces
{
    public interface IFileReader
    {
        string[] ReadFileLines(string path);
    }
}
