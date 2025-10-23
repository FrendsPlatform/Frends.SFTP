namespace Frends.SFTP.DownloadFiles.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Options for file transfer
/// </summary>
public class Options
{
    /// <summary>
    /// Timeout in seconds when the Task is cancelled after.
    /// Cancellation with the Timeout will stop the operation even if the transfer is occuring.
    /// Task will try to restore the source file. Destination file will be deleted if the cancellation happens during the transfer.
    /// Versus to the Connection.ConnectionTimeout which will only timeout the connection to the SFTP server if client is idle.
    /// Number zero (0) or negative number will disable the timeout.
    /// </summary>
    /// <example>30</example>
    [DefaultValue(0)]
    public int Timeout { get; set; }

    /// <summary>
    /// Should an exception be thrown when file transfer fails.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFail { get; set; } = true;

    /// <summary>
    /// Should the source file be renamed with temporary file name during file transfer as a locking mechanism.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool RenameSourceFileBeforeTransfer { get; set; } = true;

    /// <summary>
    /// File extension for the temporary source file which is used during the transfer.
    /// </summary>
    /// <example>.8CO</example>
    [DefaultValue(".8CO")]
    [UIHint(nameof(RenameSourceFileBeforeTransfer), "", true)]
    [DisplayFormat(DataFormatString = "Text")]
    public string SourceFileExtension { get; set; }

    /// <summary>
    /// Should the destination file be renamed with temporary file name during file transfer as a locking mechanism.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool RenameDestinationFileDuringTransfer { get; set; } = true;

    /// <summary>
    /// File extension for the temporary destination file which is used during the transfer.
    /// </summary>
    /// <example>.8CO</example>
    [DefaultValue(".8CO")]
    [UIHint(nameof(RenameDestinationFileDuringTransfer), "", true)]
    [DisplayFormat(DataFormatString = "Text")]
    public string DestinationFileExtension { get; set; }

    /// <summary>
    /// Should the destination directories be created if they do not exist. May not work on all servers.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool CreateDestinationDirectories { get; set; }

    /// <summary>
    /// Should the Last Modified timestamp be preserved from the source.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool PreserveLastModified { get; set; }

    /// <summary>
    /// While enabled all operations logs of executions will be returned with the result.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool OperationLog { get; set; } = true;
}