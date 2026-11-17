using Renci.SshNet.Sftp;

namespace Frends.SFTP.UploadFiles.Definitions;

internal class FileItem
{
    public FileItem(ISftpFile file)
    {
        Modified = file.LastWriteTime;
        Name = file.Name;
        Size = file.Length;
        FullPath = file.FullName;
        RelativeDirectoryPath = string.Empty;
    }

    public FileItem(string fullPath, string relativeDirectoryPath = "")
    {
        var fi = new FileInfo(fullPath);
        Modified = fi.LastWriteTime;
        Name = Path.GetFileName(fullPath);
        Size = TryGetFileLength(fi) ? fi.Length : 0;
        FullPath = fullPath;
        RelativeDirectoryPath = relativeDirectoryPath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileItem"/> class.
    /// Default constructor, use only for testing.
    /// </summary>
    public FileItem()
    {
    }

    /// <summary>
    /// Last modified time of the file.
    /// </summary>
    /// <example>2026-11-09T11:45:00Z</example>
    public DateTime Modified { get; set; }

    /// <summary>
    /// Name of the file without its directory path.
    /// </summary>
    /// <example>invoice-2026-11.csv</example>
    public string Name { get; set; }

    /// <summary>
    /// Full path of the file at its endpoint.
    /// </summary>
    /// <example>/outbound/invoice-2026-11.csv</example>
    public string FullPath { get; set; }

    /// <summary>
    /// Size of the file in bytes.
    /// </summary>
    /// <example>2048</example>
    public long Size { get; set; }

    /// <summary>
    /// Path of the file's directory relative to the configured source directory.
    /// </summary>
    /// <example>monthly/november</example>
    public string RelativeDirectoryPath { get; set; }

    private static bool TryGetFileLength(FileInfo fi)
    {
        try
        {
            var length = fi.Length;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
