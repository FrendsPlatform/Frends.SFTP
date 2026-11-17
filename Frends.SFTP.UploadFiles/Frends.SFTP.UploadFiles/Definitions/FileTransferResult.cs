namespace Frends.SFTP.UploadFiles.Definitions;

internal class FileTransferResult
{
    internal bool ActionSkipped { get; set; }

    internal bool Success { get; set; }

    internal string UserResultMessage { get; set; }

    internal int SuccessfulTransferCount { get; set; }

    internal int FailedTransferCount { get; set; }

    internal IEnumerable<string> TransferredFileNames { get; set; }

    internal Dictionary<string, IList<string>> TransferErrors { get; set; }

    internal IEnumerable<string> TransferredFilePaths { get; set; }

    internal string[] TransferredDestinationFilePaths { get; set; }

    internal IDictionary<string, string> OperationsLog { get; set; }
}
