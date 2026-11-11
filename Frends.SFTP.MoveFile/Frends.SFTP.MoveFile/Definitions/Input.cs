using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile.Definitions;

/// <summary>
/// Source transfer options.
/// </summary>
public class Input
{
    /// <summary>
    /// Source directory.
    /// </summary>
    /// <example><c>/source</c>.</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Directory { get; set; }

    /// <summary>
    /// Pattern to match for files. The file mask uses regular expressions, but for convenience, it has special handling for * and ? wildcards.
    /// </summary>
    /// <example><c>*.xml</c>.</example>
    [DefaultValue("*.xml")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Pattern { get; set; }

    /// <summary>
    /// Target directory where the found files should be copied to.
    /// </summary>
    /// <example><c>/destination</c>.</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string TargetDirectory { get; set; }

    /// <summary>
    /// Operation for if the transferred file exists in destination.
    /// </summary>
    /// <example><c>FileExistsOperation.Throw</c>.</example>
    [DefaultValue(FileExistsOperation.Throw)]
    public FileExistsOperation IfTargetFileExists { get; set; }

    /// <summary>
    /// Enables the Task to create the target directories.
    /// </summary>
    /// <example><c>true</c>.</example>
    [DefaultValue(true)]
    public bool CreateTargetDirectories { get; set; }

    /// <summary>
    /// Encoding for the read content.
    /// Select Other to specify an encoding by name.
    /// </summary>
    /// <example><c>FileEncoding.ANSI</c>.</example>
    [DefaultValue(FileEncoding.ANSI)]
    public FileEncoding FileEncoding { get; set; }

    /// <summary>
    /// Additional option for UTF-8 encoding to enable bom.
    /// </summary>
    /// <example><c>true</c>.</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.UTF8)]
    [DefaultValue(false)]
    public bool EnableBom { get; set; }

    /// <summary>
    /// File encoding to be used.
    /// Unicode encodings are not supported; use a code-page encoding.
    /// A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List.
    /// </summary>
    /// <example><c>utf-8</c>.</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.Other)]
    public string EncodingInString { get; set; }
}
