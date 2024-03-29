﻿using NUnit.Framework;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    class SourceOperationTests : UploadFilesTestBase
    {
        [Test]
        public async Task UploadFiles_TestSourceOperationWithMove()
        {
            var to = Path.Combine(_workDir, "uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationWithRename()
        {
            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%.txt"
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(_workDir, "uploaded_SFTPUploadTestFile1.txt")));
        }

        [Test]
        public async Task UploadFiles_SourceOperationNothingWithRenamingDisable()
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
            var result = await SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);

            Assert.IsTrue(File.Exists(Path.Combine(_workDir, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationMoveWithRenameFilesDuringTransferDisabled()
        {
            var to = Path.Combine(_workDir, "destination");
            Directory.CreateDirectory(to);
            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationRenameWithRenameFilesDuringTransferDisabled()
        {
            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%%SourceFileExtension%"
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(_workDir, "uploaded_" + source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationMoveWithRenameSourceAndDestinationFilesEnabled()
        {
            var to = Path.Combine(_workDir, "destination");
            Directory.CreateDirectory(to);

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
                Directory = _workDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationRenameWithRenameFilesDuringTransferWithRenameSourceAndDestinationFilesEnabled()
        {
            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%%SourceFileExtension%"
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(_workDir, "uploaded_" + source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestWithSourceMoveToNonExistingDirectoryShouldReturnUnsuccessfulTransfer()
        {
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
                DirectoryToMoveAfterTransfer = Path.Combine(_workDir, "uploaded"),
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);

            Assert.IsTrue(File.Exists(Path.Combine(_workDir, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationWithRenameWithDifferentDirectory()
        {
            var to = Path.Combine(_workDir, "uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = Path.Combine(to, "uploaded_%SourceFileName%.txt")
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, "uploaded_SFTPUploadTestFile1.txt")));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationWithRenameWithDifferentDirectoryWithMacrosInDirectory()
        {
            var year = DateTime.Now.Year.ToString();
            var to = Path.Combine(_workDir, $"{year}_uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = Path.Combine(_workDir + "%Year%_uploaded", "uploaded_%SourceFileName%.txt")
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, "uploaded_SFTPUploadTestFile1.txt")));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationWithRenameWithDifferentDirectoryWithMacrosInFileName()
        {
            var year = DateTime.Now.Year.ToString();
            var to = Path.Combine(_workDir, $"{year}_uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = Path.Combine(to, "uploaded_%SourceFileName%.txt")
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, "uploaded_SFTPUploadTestFile1.txt")));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationMoveThat8COFilesAreNotLeft()
        {
            var to = Path.Combine(_workDir, "uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            var files = Directory.GetFiles(to, "*.8CO").ToList();
            Assert.AreEqual(0, files.Count());
            files = Directory.GetFiles(to, "*").ToList();
            Assert.IsTrue(files.Contains(Path.Combine(to, _source.FileName)));
        }

        [Test]
        public void UploadFiles_TestSourceOperationMoveFailedAnd8COFilesAreNotLeft()
        {
            var to = Path.Combine(_workDir, "uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to
            };

            File.Copy(Path.Combine(_workDir, source.FileName), Path.Combine(to, source.FileName));

            Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));

            var files = Directory.GetFiles(_workDir, "frends_*").ToList();
            Assert.AreEqual(0, files.Count());
            files = Directory.GetFiles(_workDir, "*").ToList();
            Assert.IsTrue(files.Contains(Path.Combine(_workDir, _source.FileName)));
        }
    }
}



