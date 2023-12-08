using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    class AppendTests : UploadFilesTestBase
    {
        [Test]
        public async Task UploadFiles_AppendingToExistingFile()
        {
            var result = await SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var fullPath = _destination.Directory + "/" + _source.FileName;
            var content1 = Helpers.GetTransferredFileContent(fullPath);

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile1.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = await SFTP.UploadFiles(source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = Helpers.GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1, content2);
        }

        [Test]
        public async Task UploadFiles_AppendingToExistingFileRenameSourceFile()
        {
            var result = await SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var fullPath = _destination.Directory + "/" + _source.FileName;
            var content1 = Helpers.GetTransferredFileContent(fullPath);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile1.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = await SFTP.UploadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = Helpers.GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1, content2);
        }

        [Test]
        public async Task UploadFiles_AppendingToExistingFileRenameDestinationFile()
        {
            var result = await SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var fullPath = _destination.Directory + "/" + _source.FileName;
            var content1 = Helpers.GetTransferredFileContent(fullPath);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile1.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = await SFTP.UploadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = Helpers.GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1, content2);
        }

        [Test]
        public async Task UploadFiles_AppendingToExistingFileRenameBoth()
        {
            var result = await SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var fullPath = _destination.Directory + "/" + _source.FileName;
            var content1 = Helpers.GetTransferredFileContent(fullPath);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile1.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = await SFTP.UploadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = Helpers.GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1, content2);
        }

        [Test]
        public async Task UploadFiles_AppendingToExistingFileRenameBothWithSourceFileNameStar()
        {
            var result = await SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var fullPath = _destination.Directory + "/" + _source.FileName;
            var content1 = Helpers.GetTransferredFileContent(fullPath);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile1.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = await SFTP.UploadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = Helpers.GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1, content2);
        }
    }
}



