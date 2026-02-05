using System.ComponentModel;

namespace Frends.SFTP.ReadFile.Definitions;

/// <summary>
/// Class for task options.
/// </summary>
public class Options
{
    /// <summary>
    /// Determines if the content of the file is returned as string or byte array.
    /// </summary>
    /// <example>ContentType.Text</example>
    [DefaultValue(true)]
    public ContentType ContentType { get; set; } = ContentType.Text;
}
