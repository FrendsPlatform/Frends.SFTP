using Renci.SshNet.Sftp;

namespace Frends.SFTP.UploadFiles.Definitions;

internal class FileItem
{
    public DateTime Modified { get; set; }

    public string Name { get; set; }

    public string FullPath { get; set; }

    public long Size { get; set; }

    public string RelativeDirectoryPath { get; set; }

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
    /// Default constructor, use only for testing.
    /// </summary>
    public FileItem() { }

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

