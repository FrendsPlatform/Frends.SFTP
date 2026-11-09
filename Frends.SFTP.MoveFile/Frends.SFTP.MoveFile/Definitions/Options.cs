using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.MoveFile.Definitions;

/// <summary>
/// Additional parameters for task execution.
/// </summary>
public class Options
{
    /// <summary>
    /// Gets or sets a value indicating whether to throw an error when the task fails.
    /// </summary>
    /// <example><c>true</c>.</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets a custom message to include when the task fails.
    /// </summary>
    /// <example>File transfer failed.</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
