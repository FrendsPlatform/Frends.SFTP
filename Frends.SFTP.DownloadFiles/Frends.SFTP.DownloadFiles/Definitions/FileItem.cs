using Renci.SshNet.Sftp;

namespace Frends.SFTP.DownloadFiles.Definitions;

internal class FileItem
{
    public DateTime Modified { get; set; }

    public string Name { get; set; }

    public string FullPath { get; set; }

    public long Size { get; set; }

    public FileItem(ISftpFile file)
    {
        Modified = file.LastWriteTime;
        Name = file.Name;
        Size = file.Length;
        FullPath = file.FullName;
    }

    public FileItem(string fullPath)
    {
        var fi = new FileInfo(fullPath);
        Modified = fi.LastWriteTime;
        Name = Path.GetFileName(fullPath);
        FullPath = fullPath;
    }
}

