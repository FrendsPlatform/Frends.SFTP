namespace Frends.SFTP.DownloadFiles.Definitions;

internal class FileTransferInfo
{
    internal string TransferName { get; set; }

    internal Guid BatchId { get; set; }

    internal string SourceFile { get; set; }

    internal DateTime TransferStarted { get; set; }

    internal DateTime TransferEnded { get; set; }

    internal string DestinationFile { get; set; }

    internal long FileSize { get; set; }

    internal string ErrorInfo { get; set; }

    internal TransferResult Result { get; set; }

    internal string DestinationAddress { get; set; }

    internal string SourcePath { get; set; }

    internal string DestinationPath { get; set; }

    internal string ServiceId { get; set; }

    internal string RoutineUri { get; set; }

    internal Guid SingleFileTransferId { get; set; }

    /// <summary>
    /// Returns a string representation of the transfer info.
    /// </summary>
    /// <returns>Transfer info as a string.</returns>
    public override string ToString()
    {
        return string.Format(
        $@"{ErrorInfo}

        TransferName: {TransferName}
        BatchId: {BatchId}
        Sourcefile: {SourceFile}
        DestinationFile: {DestinationFile}
        SourcePath: {SourcePath}
        DestinationPath: {DestinationPath}
        DestinationAddress: {DestinationAddress}
        TransferStarted: {TransferStarted}
        TransferEnded: {TransferEnded}
        TransferResult: {Result}
        FileSize: {FileSize} bytes
        ServiceId: {string.Empty}
        RoutineUri: {ServiceId}");
    }
}
