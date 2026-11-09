namespace Frends.SFTP.MoveFile.Definitions;

/// <summary>
/// Error that occurred during the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Gets or sets the summary of the error.
    /// </summary>
    /// <example>Unable to move files to the destination directory.</example>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets additional information about the error.
    /// </summary>
    /// <example><c>System.IO.IOException</c>: Access denied while moving report.csv.</example>
    public Exception AdditionalInfo { get; set; }
}
