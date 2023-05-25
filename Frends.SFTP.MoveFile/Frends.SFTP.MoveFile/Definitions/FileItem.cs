namespace Frends.SFTP.MoveFile.Definitions;
/// <summary>
/// Helper class for file items to be moved in SFTP server.
/// </summary>
public class FileItem
{
    /// <summary>
    /// Source path of the moved file.
    /// </summary>
    public string SourcePath { get; set; }

    /// <summary>
    /// Destination path of the moved file.
    /// </summary>
    public string DestinationPath { get; set; }

    internal FileItem(string source, string target)
    {
        SourcePath = source;
        DestinationPath = target;
    }
}

