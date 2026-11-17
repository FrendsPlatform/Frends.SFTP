namespace Frends.SFTP.DeleteFiles.Definitions;

using System.Collections.Generic;

/// <summary>
/// Return object with private setters
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// List of file items deleted from the directory.
    /// </summary>
    /// <example>[object { Name = "report.csv", Path = "/upload/report.csv", SizeInMegaBytes = 1.25 }]</example>
    public List<FileItem> Files { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
