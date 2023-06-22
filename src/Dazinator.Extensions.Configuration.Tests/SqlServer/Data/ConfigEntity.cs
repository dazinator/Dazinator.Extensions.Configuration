namespace Dazinator.Extensions.Configuration.Tests.SqlServer.Data;

using System.ComponentModel.DataAnnotations;

public class ConfigEntity
{
    public int Id { get; set; }
    public string ConfigSectionPath { get; set; }
    public string Json { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}
