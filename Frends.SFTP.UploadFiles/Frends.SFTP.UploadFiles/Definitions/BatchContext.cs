using Renci.SshNet.Sftp;

namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// BatchContext class for creating collection of Task parameters.
/// </summary>
internal class BatchContext
{
    /// <summary>
    /// Gets or sets frends execution metadata for the current batch.
    /// </summary>
    /// <example>object { TransferName = "OutboundTransfer", WorkDir = "/tmp" }</example>
    public Info Info { get; set; }

    /// <summary>
    /// Gets or sets local working directory used while preparing transferred files.
    /// </summary>
    /// <example>/tmp/sftp-work</example>
    public string TempWorkDir { get; set; }

    /// <summary>
    /// Gets or sets options controlling the file transfer.
    /// </summary>
    /// <example>object { Timeout = 60, OperationLog = true }</example>
    public Options Options { get; set; }

    /// <summary>
    /// Gets or sets unique identifier for this task execution.
    /// </summary>
    /// <example>f52aee9b-0f38-4e88-b086-4eb90c23c04f</example>
    public Guid InstanceId { get; set; }

    /// <summary>
    /// Gets or sets name of the transfer service.
    /// </summary>
    /// <example>daily-invoice-upload</example>
    public string ServiceId { get; set; }

    /// <summary>
    /// Gets or sets files found at the source endpoint.
    /// </summary>
    /// <example>["invoice-2026-11.csv"]</example>
    public IEnumerable<FileItem> SourceFiles { get; set; }

    /// <summary>
    /// Gets or sets files found at the destination endpoint.
    /// </summary>
    /// <example>["invoice-2026-11.csv"]</example>
    public IEnumerable<ISftpFile> DestinationFiles { get; set; }

    /// <summary>
    /// Gets or sets uRI of the Frends routine that initiated this transfer.
    /// </summary>
    /// <example>https://tasks.frends.com/processes/4c90c68b-5fc0-4dc9-a776-266589eb8221</example>
    public string RoutineUri { get; set; }

    /// <summary>
    /// Gets or sets date and time when the batch transfer started.
    /// </summary>
    /// <example>2026-11-09T12:00:00Z</example>
    public DateTime BatchTransferStartTime { get; set; }

    /// <summary>
    /// Gets or sets source endpoint configuration for the batch.
    /// </summary>
    /// <example>object { Directory = "/outbound", FileName = "*.csv" }</example>
    public Source Source { get; set; }

    /// <summary>
    /// Gets or sets destination endpoint configuration for the batch.
    /// </summary>
    /// <example>object { Directory = "/incoming", FileName = "%SourceFileName%.csv" }</example>
    public Destination Destination { get; set; }

    /// <summary>
    /// Gets or sets sFTP server connection configuration.
    /// </summary>
    /// <example>object { Address = "sftp.example.com", Port = 22, UserName = "transfer-user" }</example>
    public Connection Connection { get; set; }
}
