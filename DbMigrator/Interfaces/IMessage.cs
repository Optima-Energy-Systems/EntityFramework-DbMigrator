using System;

namespace DbMigrator.Interfaces
{
    public interface IMessage
    {
        int Code { get; }
        Exception Exception { get; set; }
        string GetFormattedErrorMessage();
    }
}
