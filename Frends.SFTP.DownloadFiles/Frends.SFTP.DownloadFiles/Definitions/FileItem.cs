namespace Frends.SFTP.DownloadFiles.Definitions;

using Renci.SshNet.Sftp;

internal class FileItem
{
    internal FileItem(ISftpFile file, string relativeDirectoryPath = "")
    {
        Modified = file.LastWriteTime;
        Name = file.Name;
        Size = file.Length;
        FullPath = file.FullName;
        RelativeDirectoryPath = relativeDirectoryPath;
    }

    internal FileItem(string fullPath)
    {
        var fi = new FileInfo(fullPath);
        Modified = fi.LastWriteTime;
        Name = Path.GetFileName(fullPath);
        FullPath = fullPath;
        RelativeDirectoryPath = string.Empty;
    }

    internal DateTime Modified { get; set; }

    internal string Name { get; set; }

    internal string FullPath { get; set; }

    internal long Size { get; set; }

    internal string RelativeDirectoryPath { get; set; }
}
