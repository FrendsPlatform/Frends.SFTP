﻿namespace Frends.SFTP.DownloadFiles.Definitions;

using Renci.SshNet.Sftp;

internal class FileItem
{
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

    public DateTime Modified { get; set; }

    public string Name { get; set; }

    public string FullPath { get; set; }

    public long Size { get; set; }
}