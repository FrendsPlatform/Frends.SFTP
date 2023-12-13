using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Options for file transfer
/// </summary>
public class Options
{
    /// <summary>
    /// Timeout in seconds when the Task is cancelled after.
    /// Cancellation with the Timeout will stop the operation even if the transfer is occuring.
    /// Task will try to restore the source file. Destination file will be deleted if the cancellation happens during the transfer.
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
    public bool ThrowErrorOnFail { get; set; }

    /// <summary>
    /// Should the destination file be renamed with temporary file name during file transfer as a locking mechanism.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool RenameSourceFileBeforeTransfer { get; set; }

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
    public bool RenameDestinationFileDuringTransfer { get; set; }

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
    public bool OperationLog { get; set; }

    /// <summary>
    /// If enabled, the Task will assume that the destination file exists and skip the checking of the destination file.
    /// This option will automatically overwrite existing destination file with the same name as the transferred one.
    /// This can help transfers with server when user has limited permissions to the destination directory e.g. no permission to do lstat command.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool AssumeFileExistence { get; set; }
}


