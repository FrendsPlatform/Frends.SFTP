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
    /// Gets or sets the source directory.
    /// </summary>
    /// <example><c>/source</c>.</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Directory { get; set; }

    /// <summary>
    /// Gets or sets the file name pattern. The mask supports regular expressions and the * and ? wildcards.
    /// </summary>
    /// <example><c>*.xml</c>.</example>
    [DefaultValue("*.xml")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Pattern { get; set; }

    /// <summary>
    /// Gets or sets the destination directory for the matching files.
    /// </summary>
    /// <example><c>/destination</c>.</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string TargetDirectory { get; set; }

    /// <summary>
    /// Gets or sets the action to take if a file already exists in the destination.
    /// </summary>
    /// <example><c>FileExistsOperation.Throw</c>.</example>
    [DefaultValue(FileExistsOperation.Throw)]
    public FileExistsOperation IfTargetFileExists { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to create target directories when they do not exist.
    /// </summary>
    /// <example><c>true</c>.</example>
    [DefaultValue(true)]
    public bool CreateTargetDirectories { get; set; }

    /// <summary>
    /// Gets or sets the encoding to use for file names.
    /// Select Other to specify an encoding by name.
    /// </summary>
    /// <example><c>FileEncoding.ANSI</c>.</example>
    [DefaultValue(FileEncoding.ANSI)]
    public FileEncoding FileEncoding { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include a byte order mark when UTF-8 is selected.
    /// </summary>
    /// <example><c>true</c>.</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.UTF8)]
    [DefaultValue(false)]
    public bool EnableBom { get; set; }

    /// <summary>
    /// Gets or sets the encoding name to use when FileEncoding is Other.
    /// Unicode encodings are not supported; use a code-page encoding.
    /// A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List.
    /// </summary>
    /// <example><c>utf-8</c>.</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.Other)]
    public string EncodingInString { get; set; }
}
