namespace Frends.SFTP.DownloadFiles.Definitions;

internal class FileTransferResult
{
    public bool ActionSkipped { get; set; }

    public bool Success { get; set; }

    public string UserResultMessage { get; set; }

    public int SuccessfulTransferCount { get; set; }

    public int FailedTransferCount { get; set; }

    public IEnumerable<string> TransferredFileNames { get; set; }

    public Dictionary<string, IList<string>> TransferErrors { get; set; }

    public IEnumerable<string> TransferredFilePaths { get; set; }

    public IEnumerable<string> TransferredDestinationFilePaths { get; set; }

    public IDictionary<string, string> OperationsLog { get; set; }
}

