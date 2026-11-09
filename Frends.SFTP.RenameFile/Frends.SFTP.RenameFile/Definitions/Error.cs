namespace Frends.SFTP.RenameFile.Definitions;

/// <summary>
/// Error that occurred during the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>Unable to rename file because the source file was not found.</example>
    public string Message { get; set; }

    /// <summary>
    /// Additional information about the error.
    /// </summary>
    /// <example>SftpPathNotFoundException: File not found on the SFTP server.</example>
    public Exception AdditionalInfo { get; set; }
}
