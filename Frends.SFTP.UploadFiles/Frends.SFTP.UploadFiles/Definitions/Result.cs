namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Return object with private setters.
/// </summary>
public class Result
{
    internal Result(FileTransferResult result = null, Error error = null)
    {
        Error = error;
        if (result is null)
            return;

        ActionSkipped = result.ActionSkipped;
        Success = result.Success;
        UserResultMessage = result.UserResultMessage;
        SuccessfulTransferCount = result.SuccessfulTransferCount;
        FailedTransferCount = result.FailedTransferCount;
        TransferredFileNames = result.TransferredFileNames;
        TransferErrors = result.TransferErrors;
        TransferredFilePaths = result.TransferredFilePaths;
        TransferredDestinationFilePaths = result.TransferredDestinationFilePaths;
        OperationsLog = result.OperationsLog;
    }

    /// <summary>
    /// Gets or sets a value indicating whether boolean value of the skipped Action.
    /// </summary>
    /// <example>false.</example>
    public bool ActionSkipped { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether boolean value of the successful transfer.
    /// </summary>
    /// <example>true.</example>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets message of the transfer operations.
    /// </summary>
    /// <example>1 files transferred: test.txt".</example>
    public string UserResultMessage { get; set; }

    /// <summary>
    /// Gets or sets count of files that has been successfully transferred.
    /// </summary>
    /// <example>1.</example>
    public int SuccessfulTransferCount { get; set; }

    /// <summary>
    /// Gets or sets count of files that have not been transferred.
    /// </summary>
    /// <example>0.</example>
    public int FailedTransferCount { get; set; }

    /// <summary>
    /// Gets or sets list of transferred file names.
    /// </summary>
    /// <example>
    /// <code>
    /// [
    ///     "test.txt",
    ///     "test2.txt"
    /// ]
    /// </code>
    /// </example>
    public IEnumerable<string> TransferredFileNames { get; set; }

    /// <summary>
    /// Gets or sets dictionary of file names and errors messages of the failed transfers.
    /// </summary>
    /// <example>
    /// <code>
    /// {
    ///     test.txt :
    ///     [
    ///         Failure in CheckIfDestinationFileExists: File 'test.txt' could not be
    ///         transferred to '/upload/Upload'. Error: Unable to transfer file. Destination
    ///         file already exists: test.txt [Source file restored.],
    ///     ]
    ///     text2.txt :
    ///     [
    ///         Failure in CheckIfDestinationFileExists: File 'test2.txt' could not be
    ///         transferred to '/upload/Upload'. Error: Unable to transfer file. Destination
    ///         file already exists: test2.txt [Source file restored.],
    ///     ]
    /// }
    /// </code>
    /// </example>
    public Dictionary<string, IList<string>> TransferErrors { get; set; }

    /// <summary>
    /// Gets or sets list of transferred file paths.
    /// </summary>
    /// <example>
    /// <code>
    /// [
    ///     "C:\\test\\test.txt",
    ///     "C:\\test\\test2.txt"
    /// ]
    /// </code>
    /// </example>
    public IEnumerable<string> TransferredFilePaths { get; set; }

    /// <summary>
    /// Gets or sets list of destaintion file paths of the transferred files.
    /// </summary>
    /// <example>
    /// <code>
    /// [
    ///     "upload/Upload/test.txt",
    ///     "upload/Upload/test2.txt"
    /// ]
    /// </code>
    /// </example>
    public string[] TransferredDestinationFilePaths { get; set; }

    /// <summary>
    /// Gets or sets operations logs for the transfer.
    /// </summary>
    /// <example>
    /// <code>
    /// {
    ///     "2022-05-30 12:27:35.00Z": "FILE LIST C:\\test\\test.txt"
    ///     "2022-06-01 11:01:50.40Z": "RenameSourceFileBeforeTransfer: Renaming source file test.txt to temporary file name frends_637896781104694806az33q4kf.8CO before transfer"
    /// }
    /// </code>
    /// </example>
    public IDictionary<string, string> OperationsLog { get; set; }

    /// <summary>
    /// Gets or sets error that occurred during the task.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }.</example>
    public Error Error { get; set; }
}
