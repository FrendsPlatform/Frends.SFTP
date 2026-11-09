namespace Frends.SFTP.DownloadFiles.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Source file location.
    /// </summary>
    /// <example>Source object</example>
    public Source Source { get; set; } = new Source();

    /// <summary>
    /// Destination directory location.
    /// </summary>
    /// <example>Destination object</example>
    public Destination Destination { get; set; } = new Destination();

    /// <summary>
    /// Transfer info parameters.
    /// </summary>
    /// <example>Info object</example>
    public Info Info { get; set; } = new Info();
}
