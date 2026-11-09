namespace Frends.SFTP.ReadFile.Definitions;

/// <summary>
/// Error that occurred during the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>Unable to read the requested SFTP file.</example>
    public string Message { get; set; }

    /// <summary>
    /// Additional information about the error.
    /// </summary>
    /// <example>System.IO.IOException: Connection timed out</example>
    public Exception AdditionalInfo { get; set; }
}
