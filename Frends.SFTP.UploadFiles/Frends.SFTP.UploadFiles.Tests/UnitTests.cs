using NUnit.Framework;
using System.IO;
using System;
using System.Net;
using System.Text;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Renci.SshNet.Common;
using Frends.SFTP.UploadFiles.Definitions;
using System.Linq;

namespace Frends.SFTP.UploadFiles.Tests
{
    /// <summary>
    /// NOTE: To run these unit tests, you need an SFTP test server.
    /// 
    /// docker run -p 22:22 -d atmoz/sftp foo:pass:::upload
    /// 
    /// </summary>
    [TestFixture]
    class TestClass
    {
        /// <summary>
        /// Test credentials for docker server
        /// </summary>
        private static string _dockerAddress = Dns.GetHostName();
        private static string _dockerUsername = "foo";
        private static string _dockerPassword = "pass";

        /// <summary>
        /// Test credentials for HiQ test server
        /// </summary>
        private static string _HiQOpsAddress = Environment.GetEnvironmentVariable("HiQ_OpsTestSftpServerAddress");
        private static string _HiQOpsUsername = Environment.GetEnvironmentVariable("HiQ_OpsTestSftpServerUsername");
        private static string _HiQOpsPassword = Environment.GetEnvironmentVariable("HiQ_OpsTestSftpServerPassword");

        private static Connection _connection;
        private static Source _source;
        private static Destination _destination;
        private static Options _options;
        private static Info _info;
        private static string _workDir;
        private static string _testResultFile = "testResultFile.txt";

        [OneTimeSetUp]
        public static void Setup()
        {
            _workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");

            _connection = new Connection
            {
                ConnectionTimeout = 60,
                Address = _dockerAddress,
                Port = 2222,
                UserName = _dockerUsername,
                Authentication = AuthenticationType.UsernamePassword,
                Password = _dockerPassword,
                ServerFingerPrint = null,
                BufferSize = 32
            };

            _source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            _destination = new Destination
            {
                Directory = "/upload/Upload",
                Action = DestinationAction.Error
            };

            _options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            _info = new Info
            {
                WorkDir = null,
            };
        }

        [Test]
        public void UploadFiles()
        {
            var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFilesTestLargerBuffers()
        {
            var connection = new Connection
            {
                ConnectionTimeout = 60,
                Address = _dockerAddress,
                Port = 2222,
                UserName = _dockerUsername,
                Authentication = AuthenticationType.UsernamePassword,
                Password = _dockerPassword,
                BufferSize = 256
            };

            var source = new Source
            {
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/"),
                FileName = "LargeTestFile.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test] 
        public void UploadFilesOperationLogDisabled()
        {
            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = false
            };

            var result = SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.OperationsLog.Count);
        }

        [Test]
        public void UploadFilesWithMultipleSubdirectoriesInDestination()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload/sub",
                Action = DestinationAction.Error
            };

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFilesThatExistsThrowsError()
        {
            var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);

            var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken()));
            Assert.That(ex.Message.StartsWith($"SFTP transfer failed: 1 Errors: Failure in CheckIfDestination"));
        }

        [Test]
        public void UploadFiles_TestOneErrorInTransferWithMultipleFiles()
        {
            var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                Action = DestinationAction.Error
            };

            var options = new Options
            {
                ThrowErrorOnFail = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = SFTP.UploadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(4, result.SuccessfulTransferCount);
            Assert.AreEqual(1, result.FailedTransferCount);
        }

        [Test]
        public void UploadFile_TestSingleFileTransferError()
        {
            var options = new Options
            {
                ThrowErrorOnFail = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };
            var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);

            result = SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.That(result.FailedTransferCount == 1);

        }

        [Test]
        public void UploadFilesThrowsIfFileNotExist()
        {
            var source = new Source
            {
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/"),
                FileName = "FileThatDontExist.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
            Assert.That(ex.Message.StartsWith("SFTP transfer failed:"));
        }

        [Test]
        public void UploadFilesThrowsWithIncorrectCredentials()
        {
            var connection = new Connection
            {
                ConnectionTimeout = 10,
                Address = _dockerAddress,
                Port = 22,
                UserName = "demo",
                Authentication = AuthenticationType.UsernamePassword,
                Password = "demo",
                BufferSize = 32
            };
            Assert.Throws<Exception>(() => SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        }

        [Test]
        public void UploadFiles_TestThrowsWithWrongPort()
        {
            var connection = new Connection
            {
                ConnectionTimeout = 10,
                Address = _dockerAddress,
                Port = 51651,
                UserName = _dockerUsername,
                Authentication = AuthenticationType.UsernamePassword,
                Password = _dockerPassword,
                BufferSize = 32
            };

            var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.That(ex.Message.StartsWith("SFTP transfer failed: Unable to establish the socket: No such host is known"));
        }

        [Test]
        public void UploadFiles_TestWithFileMaskWithFileAlreadyInDestination()
        {
            var source = new Source
            {
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/"),
                FileName = "*File.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var options = new Options
            {
                ThrowErrorOnFail = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var result = SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            source.FileName = "*.txt";
            result = SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FailedTransferCount);
            Assert.AreEqual(4, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TestUsingMacros()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "%SourceFileName%%Date%%SourceFileExtension%",
                Action = DestinationAction.Error
            };

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var date = DateTime.Now;
            Assert.IsTrue(CheckFileExistsInDestination("/upload/Upload/SFTPUploadTestFile" + date.ToString(@"yyyy-MM-dd") + ".txt"));
        }

        [Test]
        public void UploadFiles_TestAppendToExistingFile()
        {
            var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var fullPath = _destination.Directory + "/" + _source.FileName;
            var content1 = GetTransferredFileContent(fullPath);

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile.txt",
                Action = DestinationAction.Append
            };

            var source = new Source
            {
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/"),
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = SFTP.UploadFiles(source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1.Length, content2.Length);
        }

        [TearDown]
        public void TearDown()
        {
            using (var sftp = new SftpClient(_connection.Address, _connection.Port, _connection.UserName, _connection.Password))
            {
                sftp.Connect();
                if (sftp.Exists(_destination.Directory))
                {
                    DeleteDirectory(sftp, _destination.Directory);
                }
                sftp.Disconnect();
                if (File.Exists(Path.Combine(_workDir, _testResultFile)))
                    File.Delete(Path.Combine(_workDir, _testResultFile));
            }
        }

        private bool CheckFileExistsInDestination(string fullpath)
        {
            using (var sftp = new SftpClient(_connection.Address, _connection.Port, _connection.UserName, _connection.Password))
            {
                var exists = true;
                sftp.Connect();
                if (!sftp.Exists(fullpath))
                    exists = false;
                sftp.Disconnect();
                return exists;
            }
        }

        private static void DeleteDirectory(SftpClient client, string path)
        {
            foreach (var file in client.ListDirectory(path))
            {
                if ((file.Name != ".") && (file.Name != ".."))
                {
                    if (file.IsDirectory)
                    {
                        DeleteDirectory(client, file.FullName);
                    }
                    else
                    {
                        client.DeleteFile(file.FullName);
                    }
                }
            }
            if (client.Exists(path))
                client.DeleteDirectory(path);
        }

        private string GetTransferredFileContent(string fullPath)
        {
            var testfile = Path.Combine(_workDir, _testResultFile);
            using (var sftp = new SftpClient(_connection.Address, _connection.Port, _connection.UserName, _connection.Password))
            {
                sftp.Connect();

                sftp.ChangeDirectory(_destination.Directory);
                using (var file = File.OpenWrite(testfile))
                {
                    sftp.DownloadFile(fullPath, file);
                }
                sftp.Disconnect();
                sftp.Dispose();
            }

            return File.ReadAllText(testfile);
        }
    }
}
