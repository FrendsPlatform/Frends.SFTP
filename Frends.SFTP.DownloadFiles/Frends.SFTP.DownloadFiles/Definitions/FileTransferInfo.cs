namespace Frends.SFTP.DownloadFiles.Definitions;

internal class FileTransferInfo
{
    public string TransferName { get; set; }

    public Guid BatchId { get; set; }

    public string SourceFile { get; set; }

    public DateTime TransferStarted { get; set; }

    public DateTime TransferEnded { get; set; }

    public string DestinationFile { get; set; }

    public long FileSize { get; set; }

    public string ErrorInfo { get; set; }

    public TransferResult Result { get; set; }

    public string DestinationAddress { get; set; }

    public string SourcePath { get; set; }

    public string DestinationPath { get; set; }

    public string ServiceId { get; set; }

    public string RoutineUri { get; set; }

    public Guid SingleFileTransferId { get; set; }

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