namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Source, destination, and transfer metadata parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Gets or sets source file location and post-transfer actions.
    /// </summary>
    /// <example>object { string Directory = "/outbound", string FileName = "*.csv" }</example>
    public Source Source { get; set; }

    /// <summary>
    /// Gets or sets destination directory and file transfer settings.
    /// </summary>
    /// <example>object { string Directory = "/incoming", string FileName = "%SourceFileName%.csv" }</example>
    public Destination Destination { get; set; }

    /// <summary>
    /// Gets or sets execution metadata supplied by Frends.
    /// </summary>
    /// <example>object { string TransferName = "OutboundTransfer", string WorkDir = "/tmp" }</example>
    public Info Info { get; set; }
}
