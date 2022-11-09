using Renci.SshNet.Sftp;

namespace Frends.SFTP.ListFiles.Definitions;

/// <summary>
/// Return object with private setters.
/// </summary>
public class FileItem
{
    /// <summary>
    /// Full path of directory or file.
    /// </summary>
    /// <example>/directory/test.txt</example>
    public string FullPath { get; private set; }

    /// <summary>
    /// Boolean value of Result object being directory.
    /// </summary>
    /// <example>false</example>
    public bool IsDirectory { get; private set; }

    /// <summary>
    /// Boolean value of Result object being file.
    /// </summary>
    /// <example>true</example>
    public bool IsFile { get; private set; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    /// <example>4</example>
    public long Length { get; private set; }

    /// <summary>
    /// Name of the file with extension.
    /// </summary>
    /// <example>test.txt</example>
    public string Name { get; private set; }

    /// <summary>
    /// Timestamps for last write in UTC timezone.
    /// </summary>
    /// <example>2022-06-28T11:05:58Z</example>
    public DateTime LastWriteTimeUtc { get; private set; }

    /// <summary>
    /// Timestamps for last access in UTC timezone.
    /// </summary>
    /// <example>2022-06-28T11:05:58Z</example>
    public DateTime LastAccessTimeUtc { get; private set; }

    /// <summary>
    /// Timestamps for last write in current timezone.
    /// </summary>
    /// <example>2022-06-28T14:05:58+03:00</example>
    public DateTime LastWriteTime { get; private set; }

    /// <summary>
    /// Timestamps for last access in current timezone.
    /// </summary>
    /// <example>2022-06-28T14:05:58+03:00</example>
    public DateTime LastAccessTime { get; private set; }

    internal FileItem(SftpFile file)
    {
        FullPath = file.FullName;
        IsDirectory = file.IsDirectory;
        IsFile = file.IsRegularFile;
        Length = file.Length;
        Name = file.Name;
        LastWriteTimeUtc = file.LastWriteTimeUtc;
        LastAccessTimeUtc = file.LastAccessTimeUtc;
        LastWriteTime = file.LastWriteTime;
        LastAccessTime = file.LastAccessTime;
    }
}

