namespace Frends.SFTP.DownloadFiles.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Boolean value of the skipped Action.
    /// </summary>
    /// <example>false</example>
    public bool ActionSkipped { get; set; }

    /// <summary>
    /// Boolean value of the successful transfer.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Message of the transfer operations.
    /// </summary>
    /// <example>1 files transferred: test.txt</example>
    public string UserResultMessage { get; set; }

    /// <summary>
    /// Count of files that has been successfully transferred.
    /// </summary>
    /// <example>1</example>
    public int SuccessfulTransferCount { get; set; }

    /// <summary>
    /// Count of files that have not been transferred.
    /// </summary>
    /// <example>0</example>
    public int FailedTransferCount { get; set; }

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
    public IEnumerable<string> TransferredFileNames { get; set; }

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
    public Dictionary<string, IList<string>> TransferErrors { get; set; }

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
    public IEnumerable<string> TransferredFilePaths { get; set; }

    /// <summary>
    /// List of destination file paths of the transferred files.
    /// </summary>
    /// <example>
    /// [
    ///     "C:\\test\\test.txt",
    ///     "C:\\test\\test2.txt"
    /// ]
    /// </example>
    public string[] TransferredDestinationFilePaths { get; set; }

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

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
