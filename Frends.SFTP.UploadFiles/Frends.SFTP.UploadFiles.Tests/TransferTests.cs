using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Frends.SFTP.UploadFiles.Definitions;


namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    class TransferTests : UploadFilesTestBase
    {

        [Test]
        public async Task UploadFiles_TestSimpleTransfer()
        {
            var result = await SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.AreEqual(Path.Combine(_destination.Directory, _source.FileName).Replace("\\", "/"), result.TransferredDestinationFilePaths.ToList().FirstOrDefault());
        }

        [Test]
        public async Task UploadFiles_TestThat8COFilesAreNotLeft()
        {
            await SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            var files = Directory.GetFiles(_workDir, "*.8CO").ToList();
            Assert.AreEqual(0, files.Count());
            files = Directory.GetFiles(_workDir, "*").ToList();
            Assert.IsTrue(files.Contains(Path.Combine(_workDir, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestUploadWithFileMaskEverything()
        {
            var source = new Source
            {
                Directory = _workDir,
                FileName = "*",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestUploadWithDefaultFileNameUploadsEverything()
        {
            var source = new Source
            {
                Directory = _workDir,
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestWithOperationLogDisabled()
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

            var result = await SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.OperationsLog.Count);
        }

        [Test]
        public async Task UploadFiles_TestWithMultipleSubdirectoriesInDestination()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload/sub",
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var result = await SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestOneErrorInTransferWithMultipleFiles()
        {
            var result = await SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);

            var destination = new Destination
            {
                Directory = "/upload/Upload",
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
                Directory = _workDir,
                FileName = "*.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = await SFTP.UploadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.AreEqual(1, result.FailedTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestWithFileMaskWithFileAlreadyInDestination()
        {
            var source = new Source
            {
                Directory = _workDir,
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

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            source.FileName = "*.txt";
            result = await SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FailedTransferCount);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestSingleFileTransferWithError()
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

            var destination = new Destination
            {
                Directory = _destination.Directory,
                FileName = "",
                Action = DestinationAction.Error,
            };

            Helpers.UploadSingleTestFile(_destination.Directory, Path.Combine(_workDir, "SFTPUploadTestFile1.txt"));

            var result = await SFTP.UploadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FailedTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestUploadWithOverwrite()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile1.txt",
                Action = DestinationAction.Overwrite,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            Helpers.UploadSingleTestFile(destination.Directory, Path.Combine(_workDir, _source.FileName));

            var result = await SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.CheckFileExistsInDestination(destination.Directory + "/" + _source.FileName));
        }

        [Test]
        public async Task UploadFiles_TestUploadWithOnlyRenameSourceDuringTransferEnabled()
        {
            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var result = await SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.CheckFileExistsInDestination(_destination.Directory + "/" + _source.FileName));
        }

        [Test]
        public async Task UploadFiles_TestUploadWithOnlyRenameDestinationDuringTransferEnabled()
        {
            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var result = await SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.CheckFileExistsInDestination(_destination.Directory + "/" + _source.FileName));
        }

        [Test]
        public async Task UploadFiles_NoSourceFilesAndIgnoreShouldNotThrowException()
        {
            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "NonExistingFile.txt",
                Action = SourceAction.Ignore,
                Operation = SourceOperation.Delete
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ActionSkipped);
        }

        [Test]
        public async Task UploadFiles_NoSourceFilesAndInfoShouldNotThrowException()
        {
            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "NonExistingFile.txt",
                Action = SourceAction.Info,
                Operation = SourceOperation.Delete
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ActionSkipped);
        }

        [Test]
        public async Task UploadFiles_MassTransferTest()
        {
            Helpers.CreateDummyFiles(30);
            Helpers.CopyLargeTestFile(10);

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*",
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing
            };

            var connection = new Connection
            {
                ConnectionTimeout = _connection.ConnectionTimeout,
                Address = _connection.Address,
                Port = _connection.Port,
                Authentication = _connection.Authentication,
                UserName = _connection.UserName,
                Password = _connection.Password,
                BufferSize = 256
            };

            var result = await SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
        }

        [Test]
        public async Task UploadFiles_TestWithFilePaths()
        {
            var filePaths = Helpers.CreateDummyFiles(3);

            var source = new Source
            {
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(3, result.SuccessfulTransferCount);
            Assert.AreNotEqual(filePaths[0], result.TransferredFileNames.ToList()[0]);
            Assert.AreEqual(Path.GetFileName(filePaths[0]), result.TransferredFileNames.ToList()[0]);
        }

        [Test]
        public async Task UploadFiles_TestWithFilePathsThatDontExist()
        {
            var paths = Helpers.CreateDummyFiles(3).ToList();
            paths.Add(@"C:\File\That\Dont\Exist.txt");
            var filePaths = paths.ToArray();

            var source = new Source
            {
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths
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

            var result = await SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ActionSkipped);
            Assert.AreEqual(3, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestWithEmptyFilePaths()
        {
            var filePaths = Array.Empty<string>();

            var source = new Source
            {
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ActionSkipped);
        }

        [Test]
        public async Task UploadFiles_TestWithFilePathsEvenIfSourceFileIsAssigned()
        {
            var filePaths = Helpers.CreateDummyFiles(3);

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*.csv",
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(3, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestWithFilePathsEvenIfSourceFileIsAssignedToAll()
        {
            var files = Helpers.CreateDummyFiles(6);

            var filePaths = new string[] { files[0], files[1], files[2] };

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*",
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(3, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestWithFilePathsObjectArray()
        {
            var files = Helpers.CreateDummyFiles(6);

            var filePaths = new object[] { files[0], files[1], files[2] };

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*",
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(3, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_ShouldThrowWithIncorrectTypedFilePaths()
        {
            var files = Helpers.CreateDummyFiles(6);

            var filePaths = new List<object> { files[0], files[1], files[2] };

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*",
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths
            };

            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
            Assert.AreEqual($"Invalid type for parameter FilePaths. Expected array but was {filePaths.GetType()}", ex.Message);
        }

        [Test]
        public async Task UploadFiles_TestNormalTransferWithTempPath()
        {
            var temp = Path.Combine(_workDir, "temp");
            Directory.CreateDirectory(temp);
            var info = new Info
            {
                WorkDir = temp
            };

            var result = await SFTP.UploadFiles(_source, _destination, _connection, _options, info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.AreEqual(0, Directory.GetFiles(temp).Length);
        }

        [Ignore("Test needs CIFS share mounted to sftp directory 'pod'")]
        [Test]
        public async Task UploadFiles_ToCIFSShare()
        {
            var destination = new Destination
            {
                Directory = "pod",
                FileName = ""
            };
            var result = await SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }
    }
}