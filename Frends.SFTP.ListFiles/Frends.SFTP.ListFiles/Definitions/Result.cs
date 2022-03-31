using System.ComponentModel.DataAnnotations;
using Renci.SshNet.Sftp;

#pragma warning disable 1591

namespace Frends.SFTP.ListFiles.Definitions
{
    /// <summary>
    /// Return object with private setters
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Full path of directory or file.
        /// </summary>
        public string FullPath { get; private set; }
        public bool IsDirectory { get; private set; }
        public bool IsFile { get; private set; }

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long Length { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// Timestamps for last access and write in both UTC and current timezone.
        /// </summary>
        public DateTime LastWriteTimeUtc { get; private set; }
        public DateTime LastAccessTimeUtc { get; private set; }
        public DateTime LastWriteTime { get; private set; }
        public DateTime LastAccessTime { get; private set; }

        public Result(SftpFile file)
        {
            this.FullPath = file.FullName;
            this.IsDirectory = file.IsDirectory;
            this.IsFile = file.IsRegularFile;
            this.Length = file.Length;
            this.Name = file.Name;
            this.LastWriteTimeUtc = file.LastWriteTimeUtc;
            this.LastAccessTimeUtc = file.LastAccessTimeUtc;
            this.LastWriteTime = file.LastWriteTime;
            this.LastAccessTime = file.LastAccessTime;
        }
    }
}
