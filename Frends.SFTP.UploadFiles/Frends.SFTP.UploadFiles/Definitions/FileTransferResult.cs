namespace Frends.SFTP.UploadFiles.Definitions;

internal class FileTransferResult
{
    /// <summary>
    /// Gets or sets a value indicating whether indicates whether the task skipped the requested action.
    /// </summary>
    /// <example>false</example>
    public bool ActionSkipped { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether indicates whether the file transfer succeeded.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets summary of the transfer outcome.
    /// </summary>
    /// <example>1 file transferred: invoice-2026-11.csv</example>
    public string UserResultMessage { get; set; }

    /// <summary>
    /// Gets or sets number of files transferred successfully.
    /// </summary>
    /// <example>1</example>
    public int SuccessfulTransferCount { get; set; }

    /// <summary>
    /// Gets or sets number of files that could not be transferred.
    /// </summary>
    /// <example>0</example>
    public int FailedTransferCount { get; set; }

    /// <summary>
    /// Gets or sets names of files transferred successfully.
    /// </summary>
    /// <example>["invoice-2026-11.csv"]</example>
    public IEnumerable<string> TransferredFileNames { get; set; }

    /// <summary>
    /// Gets or sets error messages keyed by the source file name for failed transfers.
    /// </summary>
    /// <example>{"invoice-2026-11.csv": ["Permission denied while uploading the file."]}</example>
    public Dictionary<string, IList<string>> TransferErrors { get; set; }

    /// <summary>
    /// Gets or sets full source paths of files transferred successfully.
    /// </summary>
    /// <example>["C:\\outbound\\invoice-2026-11.csv"]</example>
    public IEnumerable<string> TransferredFilePaths { get; set; }

    /// <summary>
    /// Gets or sets destination paths of files transferred successfully.
    /// </summary>
    /// <example>["/incoming/invoice-2026-11.csv"]</example>
    public string[] TransferredDestinationFilePaths { get; set; }

    /// <summary>
    /// Gets or sets transfer operation log entries keyed by timestamp.
    /// </summary>
    /// <example>{"2026-11-09 12:00:00.0Z": "Uploaded invoice-2026-11.csv"}</example>
    public IDictionary<string, string> OperationsLog { get; set; }
}
