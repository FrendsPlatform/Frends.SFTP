namespace Frends.SFTP.DownloadFiles.Definitions;

/// <summary>
/// Return object with private setters.
/// </summary>
public class Result
{
    /// <summary>
    /// Boolean value of the skipped Action.
    /// </summary>
    /// <example>false</example>
    public bool ActionSkipped { get; private set; }

    /// <summary>
    /// Boolean value of the successful transfer.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Message of the transfer operations.
    /// </summary>
    /// <example>1 files transferred: test.txt"</example>
    public string UserResultMessage { get; private set; }

    /// <summary>
    /// Count of files that has been successfully transferred.
    /// </summary>
    /// <example>1</example>
    public int SuccessfulTransferCount { get; private set; }

    /// <summary>
    /// Count of files that have not been transferred.
    /// </summary>
    /// <example>0</example>
    public int FailedTransferCount { get; private set; }

    /// <summary>
    /// List of transferred file names.
    /// </summary>
    /// <example>        
    /// <code>
    /// [
    ///     "test.txt",
    ///     "test2.txt"
    /// ]
    /// </code>
    /// </example>
    public IEnumerable<string> TransferredFileNames { get; private set; }

    /// <summary>
    /// Dictionary of file names and errors messages of the failed transfers.
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
    public Dictionary<string, IList<string>> TransferErrors { get; private set; }

    /// <summary>
    /// List of transferred file paths.
    /// </summary>
    /// <example>
    /// <code>
    /// [
    ///     "/Upload/upload/test.txt",
    ///     "/Upload/upload/test2.txt"
    /// ]
    /// </code>
    /// </example>
    public IEnumerable<string> TransferredFilePaths { get; private set; }

    /// <summary>
    /// List of destination file paths of the transferred files.
    /// </summary>
    /// <example>
    /// [
    ///     "C:\\test\\test.txt",
    ///     "C:\\test\\test2.txt"
    /// ]
    /// </example>
    public string[] TransferredDestinationFilePaths { get; private set; }

    /// <summary>
    /// Operations logs for the transfer.
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

    internal Result(FileTransferResult result)
    {
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
}

