using NUnit.Framework;
using System.IO;
using System;
using System.Threading;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    [TestFixture]
    class ErrorTests : DownloadFilesTestBase
    {
        [Test]
        public void DownloadFiles_TestTransferThatExistsThrowsError()
        {
            Directory.CreateDirectory(_destWorkDir);
            File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await SFTP.DownloadFiles(_source, _destination, _connection, _options, _info, new CancellationToken()));
            Assert.That(ex.Message.StartsWith("SFTP transfer failed: 2 Errors: Failure in GetFile"),
                $"Actual message: {ex.Message}");
        }

        [Test]
        public void DownloadFiles_TestTransferThatThrowsIfFileNotExist()
        {
            var source = new Source
            {
                Directory = "/upload",
                FileName = "FileThatDontExist.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(
                ex.Message.StartsWith("SFTP transfer failed: 1 Errors: No source files found from directory"));
        }

        [Test]
        public void DownloadFiles_TestThrowsWithWrongPort()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Port = 51651;

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(
                ex.Message.StartsWith("SFTP transfer failed: Unable to establish the socket: No such host is known"));
        }

        [Test]
        public void DownloadFiles_TestWithSubDirNameAsFileMask()
        {
            var path = "/upload/test";
            Helpers.CreateSubDirectory(path);
            var source = new Source
            {
                Directory = "/upload",
                FileName = "test",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(
                ex.Message.StartsWith("SFTP transfer failed: 1 Errors: No source files found from directory"));
            Helpers.DeleteSubDirectory(path);
        }

        [Test]
        public void DownloadFiles_TestThrowsWithSourceMoveToNonExistingDirectoryShouldReturnUnsuccessfulTransfer()
        {
            Directory.CreateDirectory(_destWorkDir);

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = "/upload/test"
            };

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(ex.Message.Contains(
                $"Operation failed: Source file {_source.FileName} couldn't be moved to given directory {source.DirectoryToMoveAfterTransfer} because the directory didn't exist."));
            Assert.IsTrue(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
        }

        [Test]
        public void DownloadFiles_TestThrowsSourceMoveToDestinationFileExists()
        {
            Directory.CreateDirectory(_destWorkDir);
            Helpers.CreateSubDirectory("/upload/uploaded");
            File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = "/upload/uploaded/"
            };

            var destination = new Destination
            {
                Directory = _destWorkDir,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
                Action = DestinationAction.Error,
            };

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await SFTP.DownloadFiles(source, destination, _connection, _options, _info, new CancellationToken()));
            Assert.That(ex.Message.StartsWith("SFTP transfer failed: 2 Errors: Failure in GetFile"),
                $"Actual message: {ex.Message}");
            Assert.IsTrue(
                Helpers.SourceFileExists(Path.Combine(_source.Directory, _source.FileName).Replace("\\", "/")));
        }

        [Test]
        public void DownloadFiles_TestThrowsSourceMoveToDestinationFileExistsWithRenameSourceFileBeforeTransfer()
        {
            Directory.CreateDirectory(_destWorkDir);
            Helpers.CreateSubDirectory("/upload/uploaded");
            File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = "/upload/uploaded/"
            };

            var destination = new Destination
            {
                Directory = _destWorkDir,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
                Action = DestinationAction.Error,
            };

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await SFTP.DownloadFiles(source, destination, _connection, _options, _info, new CancellationToken()));
            Assert.That(ex.Message.StartsWith("SFTP transfer failed: 2 Errors: Failure in GetFile"),
                $"Actual message: {ex.Message}");
            Assert.IsTrue(
                Helpers.SourceFileExists(Path.Combine(_source.Directory, _source.FileName).Replace("\\", "/")));
        }

        [Test]
        public void DownloadFiles_TestErrorMessage()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Password = Guid.NewGuid().ToString();

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(ex.Message.Contains(
                $"SFTP transfer failed: Authentication of SSH session failed: Permission denied (password)"));
        }

        [Test]
        public void DownloadFiles_TestCancellationToken()
        {
            Helpers.UploadLargeTestFiles(_source.Directory, 1);
            var connection = Helpers.GetSftpConnection();
            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "LargeTestFile1.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            using var cancellationTokenSource = new CancellationTokenSource(1000);
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await SFTP.DownloadFiles(source, _destination, connection, _options, _info,
                    cancellationTokenSource.Token));
            Assert.IsTrue(ex.Message.Contains("No files transferred."));
            Assert.IsTrue(ex.Message.Contains("Error: The operation was canceled.."));
            Assert.IsTrue(Helpers.SourceFileExists($"{source.Directory}/{source.FileName}"));
        }

        [Test]
        public void DownloadFiles_TestTimeout()
        {
            Helpers.UploadLargeTestFiles(_source.Directory, 1);
            var connection = Helpers.GetSftpConnection();
            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var options = new Options
            {
                Timeout = 1,
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await SFTP.DownloadFiles(source, _destination, connection, options, _info, default));
            Assert.IsTrue(ex.Message.Contains("The operation was canceled."));
            Assert.IsTrue(ex.Message.Contains("No files transferred."));
            Assert.IsTrue(Helpers.SourceFileExists($"{source.Directory}/LargeTestFile1.bin"));
        }

        [Test]
        public void DownloadFiles_ShouldFail_When_SourceMoveFileAlreadyExists_And_DestinationIsUnchanged()
        {
            Directory.CreateDirectory(_destWorkDir);
            const string archiveDir = "/upload/archive";
            Helpers.CreateSubDirectory(archiveDir);
            Helpers.UploadTestFiles(archiveDir, 1, null, [Path.Combine(_workDir, _source.FileName)]);

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = archiveDir
            };

            Assert.IsEmpty(Directory.GetFiles(_destWorkDir));
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, CancellationToken.None));

            Assert.IsTrue(ex.Message.Contains("No files transferred."));
            Assert.IsEmpty(Directory.GetFiles(_destWorkDir));
        }
    }
}