namespace Frends.SFTP.MoveFile.Definitions;

/// <summary>
/// Helper class for file items to be moved in SFTP server.
/// </summary>
public class FileItem
{
    internal FileItem(string source, string target)
    {
        SourcePath = source;
        DestinationPath = target;
    }

    /// <summary>
    /// Source path of the moved file.
    /// </summary>
    /// <example><c>/source/report.csv</c>.</example>
    public string SourcePath { get; set; }

    /// <summary>
    /// Destination path of the moved file.
    /// </summary>
    /// <example><c>/destination/report.csv</c>.</example>
    public string DestinationPath { get; set; }
}
