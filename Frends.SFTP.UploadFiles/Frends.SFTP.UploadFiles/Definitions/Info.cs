using System.ComponentModel;

namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Info class including optional info of the file transfer.
/// </summary>
public class Info
{
    /// <summary>
    /// Gets or sets optional descriptive name for the transfer.
    /// Will be included in the file transfer log
    /// entries and also in all error messages in the event log.
    /// </summary>
    /// <example>SFTPUpload.</example>
    [DefaultValue("\"SFTPUpload\"")]
    public string TransferName { get; set; }

    /// <summary>
    /// Gets or sets directory where temporary files are stored during transfer.
    /// </summary>
    /// <example>C:\temp\.</example>
    [DefaultValue("")]
    public string WorkDir { get; set; }

    /// <summary>
    /// Gets or sets the process URI, use #process.uri.
    /// </summary>
    /// <example>#process.uri.</example>
    [DefaultValue("#process.uri")]
    public string ProcessUri { get; set; }

    /// <summary>
    /// Gets or sets reference to the Task execution id, use #process.executionid.
    /// </summary>
    /// <example>#process.executionid.</example>
    [DefaultValue("#process.executionid")]
    public string TaskExecutionID { get; set; }
}
