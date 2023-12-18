using Renci.SshNet.Sftp;

namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// BatchContext class for creating collection of Task parameters
/// </summary>
internal class BatchContext
{
    public Info Info { get; set; }

    public string TempWorkDir { get; set; }

    public Options Options { get; set; }

    public Guid InstanceId { get; set; }

    public string ServiceId { get; set; }

    public IEnumerable<FileItem> SourceFiles { get; set; }

    public IEnumerable<ISftpFile> DestinationFiles { get; set; }

    public string RoutineUri { get; set; }

    public DateTime BatchTransferStartTime { get; set; }

    public Source Source { get; set; }

    public Destination Destination { get; set; }

    public Connection Connection { get; set; }
}

