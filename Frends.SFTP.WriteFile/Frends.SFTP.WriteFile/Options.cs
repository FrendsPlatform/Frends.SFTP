using System.ComponentModel;

namespace Frends.SFTP.WriteFile.Definitions;

/// <summary>
/// Options for write file.
/// </summary>
public class Options
{
    /// <summary>
    /// Should the destination directories be created if they do not exist. May not work on all servers. 
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool CreateDestinationDirectories { get; set; }
}
