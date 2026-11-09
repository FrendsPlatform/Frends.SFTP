using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
    /// Whether to throw an error when the task fails.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message when the task fails.
    /// </summary>
    /// <example>SFTP file write failed</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
