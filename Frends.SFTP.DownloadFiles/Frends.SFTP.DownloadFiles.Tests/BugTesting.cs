using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    [TestFixture]
    class BugTesting
    {
        protected static Connection _connection;
        protected static Source _source;
        protected static Destination _destination;
        protected static Options _options;
        protected static Info _info;

        readonly static string _dockerAddress = "localhost";
        readonly static string _dockerUsername = "foo";
        readonly static string _dockerPassword = "pass";

        readonly static string _dockerTestUsername = "test";
        readonly static string _dockerTestPassword = "test";


        protected static readonly string _workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");

        [SetUp]
        public void Setup()
        {
            _connection = GetSftpConnectionInfo();

            _source = new Source
            {
                Directory = "/home/foo",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            _destination = new Destination
            {
                Directory = Path.Combine(_workDir, "destination"),
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            _options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            _info = new Info
            {
                WorkDir = null,
            };

            UploadTestFiles();
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_workDir, true);
            DeleteSourceFiles();
        }

        [Test]
        public void Test()
        {
            var expectedLastAccessTime = UploadTestFiles().Attributes.LastAccessTime;
            DateTime expectedDirLasAccessTime;

            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();
                expectedDirLasAccessTime = client.GetLastAccessTime(_source.Directory);
                client.Disconnect();
            }

            DateTime lastAccessTime;
            DateTime lastAccessTimeDir;
            Thread.Sleep(60000);

            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();
                var files = client.ListDirectory(_source.Directory).ToList();
                lastAccessTime = client.GetLastAccessTime(Path.Combine(_source.Directory, _source.FileName).Replace("\\", "/"));
                lastAccessTimeDir = client.GetLastAccessTime(_source.Directory);
                client.Disconnect();
            }
            Assert.AreEqual(expectedLastAccessTime, lastAccessTime);
            Assert.AreEqual(expectedDirLasAccessTime, lastAccessTimeDir);
        }

        [Test]
        public void DownloadFiles_TestDirectoryAboveUploadDir()
        {
            var expectedLastAccessTime = UploadTestFiles().Attributes.LastAccessTime;

            var result = SFTP.DownloadFiles(_source, _destination, _connection, _options, _info, default);

            var lastAccessTime = CheckLastAccessTime(result.TransferredFilePaths.ToList()[0]);

            Assert.AreEqual(expectedLastAccessTime, lastAccessTime);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void DownloadFiles_TestRelativePaths()
        {
            UploadTestFiles("/home/foo");

            var source = new Source
            {
                Directory = "../foo",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, default);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            DeleteSourceFiles("/home/foo");
        }

        [Test]
        public void DownloadFiles_TestRelativePathsWithMove()
        {
            UploadTestFiles("/home/foo");

            var source = new Source
            {
                Directory = "../foo",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = "/home"
            };

            var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, default);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            //DeleteSourceFiles("/home/foo");
        }

        [Test]
        public void test()
        {
            var source = new Source
            {
                Directory = "//etc/ssh",
                FileName = "sshd_config",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, default);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        private static Connection GetSftpConnectionInfo()
        {
            return new Connection
            {
                ConnectionTimeout = 60,
                Address = _dockerAddress,
                Port = 2222,
                UserName = _dockerTestUsername,
                Authentication = AuthenticationType.UsernamePassword,
                Password = _dockerTestPassword,
                BufferSize = 32,
                KeepAliveInterval = -1
            };
        }

        private static SftpFile UploadTestFiles(string destination = "/home/foo")
        {
            var files = Helpers.CreateDummyFiles(3);

            List<SftpFile> fileList = new List<SftpFile>();

            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();

                var test = client.WorkingDirectory;

                foreach (var file in files)
                {
                    using (var fs = File.OpenRead(file))
                    {
                        client.UploadFile(fs, Path.Combine(destination, Path.GetFileName(file)).Replace("\\", "/"), true);
                    }
                }

                fileList = client.ListDirectory(destination).ToList();

                client.Disconnect();
            }

            return fileList[0];
        }

        private static DateTime CheckLastAccessTime(string path)
        {
            DateTime accessTime;
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();

                accessTime = client.Get(path).Attributes.LastAccessTime;

                client.Disconnect();
            }

            return accessTime;
        }

        private static void DeleteSourceFiles(string source = "/home/foo")
        {
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();
                var files = client.ListDirectory(source);

                foreach (var file in files)
                {
                    if (!file.IsDirectory)
                        client.DeleteFile(file.FullName);
                }

                client.Disconnect();
            }
        }
    }
}
