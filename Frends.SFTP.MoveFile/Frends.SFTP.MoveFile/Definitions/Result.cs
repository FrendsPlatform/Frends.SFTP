namespace Frends.SFTP.MoveFile.Definitions;

/// <summary>
/// Return object for task execution.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets or sets a value indicating whether the task completed successfully.
    /// </summary>
    /// <example><c>true</c>.</example>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the list of FileItem objects with source path and target path attributes.
    /// </summary>
    /// <example><c>[object { SourcePath: /source/test1.txt, TargetPath: /destination/test1.txt }, object { SourcePath: /source/test2.txt, TargetPath: /destination/test2.txt } ]</c>.</example>
    public List<FileItem> Files { get; set; } = new();

    /// <summary>
    /// Gets or sets the message describing the transfer.
    /// </summary>
    /// <example>Successfully moved 2 files to /destination/.</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error that occurred during task execution.
    /// </summary>
    /// <example><c>object { string Message, Exception AdditionalInfo }</c>.</example>
    public Error Error { get; set; }
}
