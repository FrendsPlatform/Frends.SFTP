using NUnit.Framework;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    [TestFixture]
    class SourceOperationTests : DownloadFilesTestBase
    {
        [Test]
        public async Task DownloadFiles_SourceOperationNothingWithRenamingDisable()
        {
            var options = new Options
            {
                ThrowErrorOnFail = false,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };
            var result = await SFTP.DownloadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);

            Assert.IsTrue(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
        }

        [Test]
        public async Task DownloadFiles_TestSourceOperationMove()
        {
            var to = "uploaded";
            Helpers.UploadTestFiles(_source.Directory, 1, to);
            to = "upload/Upload/" + to;
            var source = new Source
            {
                Directory = "upload/Upload",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.SourceFileExists(source.DirectoryToMoveAfterTransfer + "/" + source.FileName));
        }

        [Test]
        public async Task DownloadFiles_TestWithSourceMoveToNonExistingDirectoryShouldReturnUnsuccessfulTransfer()
        {
            Directory.CreateDirectory(_destWorkDir);

            var options = new Options
            {
                ThrowErrorOnFail = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = "/upload/test"
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.IsTrue(Helpers.SourceFileExists(Path.Combine(_source.Directory, _source.FileName).Replace("\\", "/")));
        }

        [Test]
        public async Task DownloadFiles_TestSourceOperationRename()
        {
            var source = new Source
            {
                Directory = "upload/Upload",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%.txt"
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.SourceFileExists(source.Directory + "/uploaded_" + source.FileName));
        }

        [Test]
        public async Task DownloadFiles_TestSourceOperationMoveWithRenameFilesDuringTransfer()
        {
            var to = "uploaded";
            Helpers.UploadTestFiles(_source.Directory, 3, to);
            to = "upload/Upload/" + to;

            var options = new Options
            {
                ThrowErrorOnFail = false,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = "upload/Upload",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.SourceFileExists(source.DirectoryToMoveAfterTransfer + "/" + source.FileName));
        }

        [Test]
        public async Task DownloadFiles_TestSourceOperationRenameWithRenameFilesDuringTransfer()
        {
            var options = new Options
            {
                ThrowErrorOnFail = false,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = "upload/Upload",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%.txt"
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.SourceFileExists(source.Directory + "/uploaded_" + source.FileName));
        }

        [Test]
        public async Task DownloadFiles_TestSourceOperationMoveWithRenameFilesDuringTransferWithRenameSourceAndDestinationFilesEnabled()
        {
            var to = "uploaded";
            Helpers.UploadTestFiles(_source.Directory, 3, to);
            to = "upload/Upload/" + to;

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
                Directory = "upload/Upload",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.SourceFileExists(source.DirectoryToMoveAfterTransfer + "/" + source.FileName));
        }

        [Test]
        public async Task DownloadFiles_TestSourceOperationRenameWithRenameFilesDuringTransferWithRenameSourceAndDestinationFilesEnabled()
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

            var source = new Source
            {
                Directory = "upload/Upload",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%.txt"
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.SourceFileExists(source.Directory + "/uploaded_" + source.FileName));
        }

        [Test]
        public async Task DownloadFiles_SourceOperationDelete()
        {
            Directory.CreateDirectory(_destWorkDir);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Delete
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
        }

        [Test]
        public async Task DownloadFiles_SourceOperationDeleteRenameSourceFile()
        {
            Directory.CreateDirectory(_destWorkDir);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Delete
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
        }

        [Test]
        public async Task DownloadFiles_SourceOperationDeleteRenameDestinationFile()
        {
            Directory.CreateDirectory(_destWorkDir);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Delete
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
        }

        [Test]
        public async Task DownloadFiles_SourceOperationDeleteRenameBoth()
        {
            Directory.CreateDirectory(_destWorkDir);

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
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Delete
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
        }

        [Test]
        public async Task DownloadFiles_TestSourceOperationRenameWithDifferentDirectory()
        {
            Helpers.CreateSubDirectory("upload/moved");

            var source = new Source
            {
                Directory = "upload/Upload",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "upload/moved/uploaded_%SourceFileName%.txt"
            };

            var result = await SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.SourceFileExists("upload/moved/uploaded_" + source.FileName));
        }
    }
}


