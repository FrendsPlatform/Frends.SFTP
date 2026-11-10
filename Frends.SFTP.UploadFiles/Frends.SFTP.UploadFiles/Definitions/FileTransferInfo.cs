namespace Frends.SFTP.UploadFiles.Definitions;

internal class FileTransferInfo
{
    /// <summary>
    /// Name of the transfer.
    /// </summary>
    /// <example>OutboundTransfer</example>
    public string TransferName { get; set; } // 0

    /// <summary>
    /// Unique identifier for the transfer batch.
    /// </summary>
    /// <example>f52aee9b-0f38-4e88-b086-4eb90c23c04f</example>
    public Guid BatchId { get; set; } // 1

    /// <summary>
    /// Name of the source file.
    /// </summary>
    /// <example>invoice-2026-11.csv</example>
    public string SourceFile { get; set; } // 2

    /// <summary>
    /// Time when the file transfer started.
    /// </summary>
    /// <example>2026-11-09T12:00:00Z</example>
    public DateTime TransferStarted { get; set; } // 3

    /// <summary>
    /// Time when the file transfer finished.
    /// </summary>
    /// <example>2026-11-09T12:00:05Z</example>
    public DateTime TransferEnded { get; set; } // 4

    /// <summary>
    /// Name assigned to the destination file.
    /// </summary>
    /// <example>invoice-2026-11.csv</example>
    public string DestinationFile { get; set; } // 5

    /// <summary>
    /// Size of the transferred file in bytes.
    /// </summary>
    /// <example>2048</example>
    public long FileSize { get; set; } // 6

    /// <summary>
    /// Details of an error encountered during transfer.
    /// </summary>
    /// <example>Permission denied while uploading invoice-2026-11.csv</example>
    public string ErrorInfo { get; set; } // 7

    /// <summary>
    /// Outcome of the file transfer.
    /// </summary>
    /// <example>TransferResult.Success</example>
    public TransferResult Result { get; set; } // 10

    /// <summary>
    /// Address of the destination SFTP server.
    /// </summary>
    /// <example>sftp.example.com</example>
    public string DestinationAddress { get; set; } // 13

    /// <summary>
    /// Path of the source file.
    /// </summary>
    /// <example>C:\\outbound\\invoice-2026-11.csv</example>
    public string SourcePath { get; set; } // 15

    /// <summary>
    /// Path of the destination file.
    /// </summary>
    /// <example>/incoming/invoice-2026-11.csv</example>
    public string DestinationPath { get; set; } // 16

    /// <summary>
    /// Identifier of the transfer service.
    /// </summary>
    /// <example>sftp-outbound</example>
    public string ServiceId { get; set; } // 17

    /// <summary>
    /// URI of the routine that initiated the transfer.
    /// </summary>
    /// <example>https://tasks.frends.com/processes/4c90c68b-5fc0-4dc9-a776-266589eb8221</example>
    public string RoutineUri { get; set; } // 18

    /// <summary>
    /// Unique identifier for the individual file transfer.
    /// </summary>
    /// <example>3a2f5ef1-23f8-44c9-8760-d8427fca83dd</example>
    public Guid SingleFileTransferId { get; set; }

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
