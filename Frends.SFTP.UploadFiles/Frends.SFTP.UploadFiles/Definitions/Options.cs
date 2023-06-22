using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Options for file transfer
/// </summary>
public class Options
{
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
    /// If enabled the operations log will be written in file while executing.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool Debug { get; set; }

    /// <summary>
    /// Directory for the operations log if Debug is enabled.
    /// </summary>
    /// <example>C:\temp\debuglog.txt</example>
    [UIHint(nameof(Debug), "", true)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("C:\\temp\\debuglog\\")]
    public string DebugDirectory { get; set; }
}


