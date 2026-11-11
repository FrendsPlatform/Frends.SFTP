using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.MoveFile.Definitions;

/// <summary>
/// Additional parameters for task execution.
/// </summary>
public class Options
{
    /// <summary>
    /// Whether to throw an error when the task fails.
    /// </summary>
    /// <example><c>true</c>.</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Custom message to include when the task fails.
    /// </summary>
    /// <example>File transfer failed.</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
