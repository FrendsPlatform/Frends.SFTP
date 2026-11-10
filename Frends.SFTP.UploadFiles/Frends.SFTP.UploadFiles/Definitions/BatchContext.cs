using Renci.SshNet.Sftp;

namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// BatchContext class for creating collection of Task parameters.
/// </summary>
internal class BatchContext
{
    internal Info Info { get; set; }

    internal string TempWorkDir { get; set; }

    internal Options Options { get; set; }

    internal Guid InstanceId { get; set; }

    internal string ServiceId { get; set; }

    internal IEnumerable<FileItem> SourceFiles { get; set; }

    internal IEnumerable<ISftpFile> DestinationFiles { get; set; }

    internal string RoutineUri { get; set; }

    internal DateTime BatchTransferStartTime { get; set; }

    internal Source Source { get; set; }

    internal Destination Destination { get; set; }

    internal Connection Connection { get; set; }
}
