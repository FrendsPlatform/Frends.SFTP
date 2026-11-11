using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
    [DefaultValue(ContentType.Text)]
    public ContentType ContentType { get; set; } = ContentType.Text;

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Unable to read the requested SFTP file</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
