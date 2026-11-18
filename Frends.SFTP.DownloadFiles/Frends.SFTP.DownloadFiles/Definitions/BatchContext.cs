namespace Frends.SFTP.DownloadFiles.Definitions;

using Renci.SshNet.Sftp;

/// <summary>
/// BatchContext class for creating collection of Task parameters
/// </summary>
internal class BatchContext
{
    internal Info Info { get; set; }

    internal string TempWorkDir { get; set; }

    internal Options Options { get; set; }

    internal Guid InstanceId { get; set; }

    internal string ServiceId { get; set; }

    internal IEnumerable<SftpFile> SourceFiles { get; set; }

    internal IEnumerable<FileItem> DestinationFiles { get; set; }

    internal string RoutineUri { get; set; }

    internal DateTime BatchTransferStartTime { get; set; }

    internal Source Source { get; set; }

    internal Destination Destination { get; set; }

    internal Connection Connection { get; set; }
}
