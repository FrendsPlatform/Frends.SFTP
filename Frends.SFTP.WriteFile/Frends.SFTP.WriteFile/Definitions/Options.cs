using System.ComponentModel;

namespace Frends.SFTP.WriteFile.Definitions;

/// <summary>
/// Options for write file.
/// </summary>
public class Options
{
    /// <summary>
    /// Should the destination directories be created if they do not exist. May not work on all servers.
    /// Note: This requires the SFTP user to have directory creation permissions on the server.
    /// May fail on servers with restricted permissions or strict security policies.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool CreateDestinationDirectories { get; set; }

    /// <summary>
    /// Verify that the file was written successfully by checking its existence and size after the write operation.
    /// Note: This requires permission to read the file on the server.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool VerifyWrite { get; set; } = true;
}
