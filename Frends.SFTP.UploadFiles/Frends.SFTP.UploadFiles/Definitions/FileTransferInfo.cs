namespace Frends.SFTP.UploadFiles.Definitions;

internal class FileTransferInfo
{
    internal string TransferName { get; set; } // 0

    internal Guid BatchId { get; set; } // 1

    internal string SourceFile { get; set; } // 2

    internal DateTime TransferStarted { get; set; } // 3

    internal DateTime TransferEnded { get; set; } // 4

    internal string DestinationFile { get; set; } // 5

    internal long FileSize { get; set; } // 6

    internal string ErrorInfo { get; set; } // 7

    internal TransferResult Result { get; set; } // 10

    internal string DestinationAddress { get; set; } // 13

    internal string SourcePath { get; set; } // 15

    internal string DestinationPath { get; set; } // 16

    internal string ServiceId { get; set; } // 17

    internal string RoutineUri { get; set; } // 18

    internal Guid SingleFileTransferId { get; set; }

    /// <summary>
    /// Formats the transfer details for diagnostic logging.
    /// </summary>
    /// <example>TransferName: OutboundTransfer; Sourcefile: invoice-2026-11.csv</example>
    /// <returns>Formatted transfer details for diagnostic logging.</returns>
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
