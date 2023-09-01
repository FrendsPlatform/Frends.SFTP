namespace Frends.SFTP.DeleteFiles.Definitions;

using Renci.SshNet.Sftp;

/// <summary>
/// Return object.
/// </summary>
public class FileItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileItem"/> class.
    /// </summary>
    /// <param name="file">SftpFile object of deleted file.</param>
    public FileItem(SftpFile file)
    {
        Name = file.Name;
        Path = file.FullName;
        SizeInMegaBytes = file.Length / 1024d / 1024d;
    }

    /// <summary>
    /// Name of the deleted file.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Full path of the deleted file.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Size of the deleted file in mega bytes.
    /// </summary>
    public double SizeInMegaBytes { get; set; }
}
