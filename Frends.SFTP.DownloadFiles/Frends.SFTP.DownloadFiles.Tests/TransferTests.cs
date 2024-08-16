using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    [TestFixture]
    class TransferTests : DownloadFilesTestBase
    {
        [Test]
        public async Task DownloadFiles_TestSimpleTransfer()
        {
            var result = await SFTP.DownloadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.AreEqual(Path.Combine(_destination.Directory, _source.FileName), result.TransferredDestinationFilePaths.ToList().FirstOrDefault());
        }

        [Test]
        public void DownloadFiles_TestTransferWithoutSourceDirectoryShouldThrow()
        {
            var source = new Source
            {
                Directory = "",
                FileName = _source.FileName,
                Action = _source.Action,
                Operation = _source.Operation,
            };

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(ex.Message.Contains("No source"));
        }

        [Test]
        public async Task DownloadFiles_TestDownloadWithFileMask()
        {
            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestWithOperationLogDisabled()
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

            var result = await SFTP.DownloadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.OperationsLog.Count);
        }

        [Test]
        public async Task DownloadFiles_TestWithMultipleSubdirectoriesInDestination()
        {
            var destination = new Destination
            {
                Directory = Path.Combine(_destWorkDir, "another\\folder"),
                Action = DestinationAction.Error,
            };

            var result = await SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_TestOneErrorInTransferWithMultipleFiles()
        {
            Directory.CreateDirectory(_destWorkDir);
            File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

            var destination = new Destination
            {
                Directory = _destWorkDir,
                Action = DestinationAction.Error,
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

            var result = await SFTP.DownloadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(2, result.SuccessfulTransferCount);
            Assert.AreEqual(1, result.FailedTransferCount);
            Assert.IsTrue(result.UserResultMessage.Contains("2 files transferred:"));
        }

        [Test]
        public async Task DownloadFiles_TestSingleFileTransferWithError()
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

            Directory.CreateDirectory(_destWorkDir);
            File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

            var result = await SFTP.DownloadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FailedTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestWithFileMaskWithFileAlreadyInDestination()
        {
            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*File1.txt",
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

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            source.FileName = "*.txt";
            result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FailedTransferCount);
            Assert.AreEqual(2, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestDownloadWithOverwrite()
        {
            var destination = new Destination
            {
                Directory = Path.Combine(_workDir, "destination"),
                Action = DestinationAction.Overwrite,
            };

            var result = await SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_TestTransferWithRenameSourceEnabledRenameDestinationDisabled()
        {
            var destination = new Destination
            {
                Directory = Path.Combine(_workDir, "destination"),
                Action = DestinationAction.Overwrite,
            };

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_NoSourceFilesAndIgnoreShouldNotThrowException()
        {
            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*.csv",
                Action = SourceAction.Ignore,
                Operation = SourceOperation.Delete
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.ActionSkipped);
            Assert.IsFalse(result.UserResultMessage.Contains("1 files transferred"));
            Assert.IsTrue(result.UserResultMessage.Contains("No files transferred"));
        }

        [Test]
        public async Task DownloadFiles_NoSourceFilesAndInfoShouldNotThrowException()
        {
            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*.csv",
                Action = SourceAction.Info,
                Operation = SourceOperation.Delete
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.ActionSkipped);
            Assert.IsFalse(result.UserResultMessage.Contains("1 files transferred"));
            Assert.IsTrue(result.UserResultMessage.Contains("No files transferred"));
        }

        [Test]
        public async Task DownloadFiles_TestNoSourceShouldNotCreateOperationsLogWhenSourceActionIsIgnore()
        {
            Directory.CreateDirectory(_destWorkDir);

            var source = new Source
            {
                Directory = "/upload/",
                FileName = _source.FileName,
                Action = SourceAction.Ignore,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.OperationsLog.Count);
        }

        [Test]
        public async Task DownloadFiles_TestWithFilePaths()
        {
            var filePaths = Helpers.UploadTestFiles(_source.Directory, 3);

            var source = new Source
            {
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.AreEqual(3, result.SuccessfulTransferCount);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(filePaths[0], result.TransferredFileNames.ToList()[0]);
            Assert.AreEqual(Path.GetFileName(filePaths[0]), result.TransferredFileNames.ToList()[0]);
        }

        [Test]
        public async Task DownloadFiles_TestWithFilePathsEvenIfSourceFileIsAssigned()
        {
            var filePaths = Helpers.UploadTestFiles(_source.Directory, 3);

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.AreEqual(3, result.SuccessfulTransferCount);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task DownloadFiles_TestWithEmptyFilePathsShouldNotThrow()
        {
            var filePaths = Array.Empty<string>();

            var source = new Source
            {
                Directory = "/",
                FileName = string.Empty,
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.AreEqual(0, result.SuccessfulTransferCount);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ActionSkipped);
        }

        [Test]
        public async Task DownloadFiles_TestWithFileNotFoundInFilePaths()
        {
            var filePaths = new string[] { "/upload/fileThatdontexists.txt" };

            var source = new Source
            {
                Directory = "/",
                FileName = string.Empty,
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.AreEqual(0, result.SuccessfulTransferCount);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ActionSkipped);
        }

        [Test]
        public async Task DownloadFiles_TestTransferWithSpecialCharactersInFileNames()
        {
            // upload test files
            var files = new List<string> { "this is a test file.txt", "This_is(a test file).txt", "this is  { a test} file.txt" };
            Helpers.UploadTestFiles(_source.Directory, 0, null, files);

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "this is a test file.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            source = new Source
            {
                Directory = _source.Directory,
                FileName = "This_is(a test file).txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            source = new Source
            {
                Directory = _source.Directory,
                FileName = "this is  { a test} file.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFile_TestTimeoutWithValue0()
        {
            Helpers.UploadLargeTestFiles(_source.Directory, 10);

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };
            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, default);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task DownloadFiles_TestWithIncludeSubdirectories()
        {
            Helpers.UploadTestFiles("/upload/other", 1, null, new List<string> { "test1.txt" });
            Helpers.UploadTestFiles("/upload/test", 1, null, new List<string> { "test2.txt" });
            Helpers.UploadTestFiles("/upload/Upload", 1, null, new List<string> { "test3.txt" });
            Helpers.UploadTestFiles("/upload/Upload/another", 1, null, new List<string> { "test4.txt" });

            var source = new Source
            {
                Directory = "/upload",
                FileName = "*",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
                IncludeSubdirectories = true
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(4, result.SuccessfulTransferCount);
        }
    }
}



