namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Error that occurred during the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Gets or sets summary of the error.
    /// </summary>
    /// <example>SFTP upload failed because the server connection was refused.</example>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets additional information about the error.
    /// </summary>
    /// <example>System.Net.Sockets.SocketException: Connection refused.</example>
    public Exception AdditionalInfo { get; set; }
}
