using System;

namespace Frends.Community.Sftp.Tests.Mocks
{
    internal class MockFileResult : IFileResult
    {
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsFile { get; set; }
        public long Length { get; set; }
        public string Name { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastAccessTime { get; set; }
    }
}
