using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile.Definitions;

/// <summary>
/// Source transfer options
/// </summary>
public class Input
{
    /// <summary>
    /// Source directory.
    /// </summary>
    /// <example>/source</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Directory { get; set; }

    /// <summary>
    /// Pattern to match for files. The file mask uses regular expressions, but for convenience, it has special handling for * and ? wildcards.
    /// </summary>
    /// <example>/destination</example>
    [DefaultValue("*.xml")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Pattern { get; set; }

    /// <summary>
    /// Target directory where the found files should be copied to.
    /// </summary>
    /// <example>/destination</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string TargetDirectory { get; set; }

    /// <summary>
    /// Operation for if the transferred file exists in destination.
    /// </summary>
    /// <example>FileExistsOperation.Throw</example>
    [DefaultValue(FileExistsOperation.Throw)]
    public FileExistsOperation IfTargetFileExists { get; set; }

    /// <summary>
    /// Enables the Task to create the target directories.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool CreateTargetDirectories { get; set; }
}
