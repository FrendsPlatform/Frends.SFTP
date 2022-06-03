using NUnit.Framework;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using Renci.SshNet.Common;
using Frends.SFTP.DownloadFiles.Definitions;
namespace Frends.SFTP.DownloadFiles.Tests
{
    /// <summary>
    /// NOTE: To run these unit tests, you need an SFTP test server.
    /// This run command will create a docker container which is used in the tests.
    /// Run this command with absolute path to the Frends.SFTP.DownloadFiles.Tests\Volumes diretory.
    /// docker-compose -f .\Frends.SFTP.DownloadFiles.Tests\docker_compose.yml up -d
    /// </summary>
    [TestFixture]
    class TestClass : DownloadFilesTestBase
    {
        [Test]
        public void DownloadFiles_TestSimpleTransfer()
        {
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

            var result = SFTP.DownloadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test] 
        public void DownloadFiles_TestWithOperationLogDisabled()
        {
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = false
            };

            var result = SFTP.DownloadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.OperationsLog.Count);
        }

        [Test]
        public void DownloadFiles_TestWithMultipleSubdirectoriesInDestination()
        {

            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

            var destination = new Destination
            {
                Directory = Path.Combine(_destWorkDir, "another\\folder"),
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.That(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
        }

        [Test]
        public void DownloadFiles_TestTransferThatExistsThrowsError()
        {
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
            Directory.CreateDirectory(_destWorkDir);
            File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

            var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, _connection, _options, _info, new CancellationToken()));
            Assert.That(ex.Message.StartsWith($"SFTP transfer failed: 1 Errors: Failure in CheckIfDestination"));
        }

        [Test]
        public void DownloadFiles_TestOneErrorInTransferWithMultipleFiles()
        {
            var files = new List<string>
            {
                Path.Combine(_workDir, "SFTPUploadTestFile.txt"),
                Path.Combine(_workDir, "SFTPUploadTestFile2.txt"),
                Path.Combine(_workDir, "SFTPUploadTestFile3.txt")
            };
            Helpers.UploadTestFiles(files, _source.Directory);
            Directory.CreateDirectory(_destWorkDir);
            File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

            var destination = new Destination
            {
                Directory = _destWorkDir,
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
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

            var result = SFTP.DownloadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(2, result.SuccessfulTransferCount);
            Assert.AreEqual(1, result.FailedTransferCount);
        }

        [Test]
        public void DownloadFiles_TestSingleFileTransferWithError()
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
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
            Directory.CreateDirectory(_destWorkDir);
            File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

            var result = SFTP.DownloadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.That(result.FailedTransferCount == 1);
        }

        [Test]
        public void DownloadFiles_TestTransferThatThrowsIfFileNotExist()
        {
            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "FileThatDontExist.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var ex = Assert.Throws<SftpPathNotFoundException>(() => SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
            Assert.That(ex.Message.StartsWith("No such file"));
        }

        [Test]
        public void DownloadFiles_TestWithFileMaskWithFileAlreadyInDestination()
        {
            var source = new Source
            {
                Directory = _source.Directory,
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

            var files = new List<string>
            {
                Path.Combine(_workDir, "SFTPUploadTestFile.txt"),
                Path.Combine(_workDir, "SFTPUploadTestFile2.txt"),
                Path.Combine(_workDir, "SFTPUploadTestFile3.txt"),
            };
            Helpers.UploadTestFiles(files, _source.Directory);

            var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            source.FileName = "*.txt";
            result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FailedTransferCount);
            Assert.AreEqual(2, result.SuccessfulTransferCount);
        }

        [Test]
        public void DownloadFiles_TestUsingMacros()
        {
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
            var destination = new Destination
            {
                Directory = _destination.Directory,
                FileName = "%SourceFileName%%Date%%SourceFileExtension%",
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var date = DateTime.Now;
            Assert.IsTrue(File.Exists(Path.Combine(_destWorkDir, "SFTPUploadTestFile" + date.ToString(@"yyyy-MM-dd") + ".txt")));
        }

        [Test]
        public void DownloadFiles_TestSourceDirectoryWithMacros()
        {
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory + "/testfolder_2022");

            var source = new Source
            {
                Directory = "/upload/Upload/testfolder_%Year%",
                FileName = "SFTPUploadTestFile.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

        }

        [Test]
        public void DownloadFiles_TestDestinationDirectoryWithMacros()
        {
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
            
            var destination = new Destination
            {
                Directory = Path.Combine(_destWorkDir, "test%Year%"),
                FileName = "",
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            var dir = Path.Combine(_destWorkDir, "test2022");

            var exists = File.Exists(Path.Combine(dir, _source.FileName));
            Assert.IsTrue(exists);
        }

        [Test]
        public void DownloadFiles_TestAppendToExistingFile()
        {
            Directory.CreateDirectory(_destWorkDir);
            File.Copy(Path.Combine(_workDir, "SFTPUploadTestFile.txt"), Path.Combine(_destWorkDir, "SFTPUploadTestFile.txt"));
            var file1 = new FileInfo(Path.Combine(_workDir, "SFTPUploadTestFile.txt"));
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, "SFTPUploadTestFile2.txt") }, _source.Directory);

            var destination = new Destination
            {
                Directory = _destWorkDir,
                FileName = "SFTPUploadTestFile.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var source = new Source
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var result = SFTP.DownloadFiles(source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var file2 = new FileInfo(Path.Combine(_destWorkDir, "SFTPUploadTestFile.txt"));
            Assert.AreNotEqual(file1.Length, file2.Length);
        }
    }
}
