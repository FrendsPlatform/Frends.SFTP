using Frends.SFTP.UploadFiles.Definitions;
using NUnit.Framework;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    internal class AppendTests : UploadFilesTestBase
    {
        [Test]
        public async Task UploadFiles_AppendingToExistingFile()
        {
            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            var fullPath = TestDestination.Directory + "/" + TestSource.FileName;
            var content1 = Helpers.GetTransferredFileContent(fullPath);

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile1.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
            };

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            result = await SFTP.UploadFiles(source, destination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            var content2 = Helpers.GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1, content2);
        }

        [Test]
        public async Task UploadFiles_AppendingToExistingFileRenameSourceFile()
        {
            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            var fullPath = TestDestination.Directory + "/" + TestSource.FileName;
            var content1 = Helpers.GetTransferredFileContent(fullPath);

            var options = new Options
            {
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true,
            };

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile1.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
            };

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            result = await SFTP.UploadFiles(source, destination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            var content2 = Helpers.GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1, content2);
        }

        [Test]
        public async Task UploadFiles_AppendingToExistingFileRenameDestinationFile()
        {
            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            var fullPath = TestDestination.Directory + "/" + TestSource.FileName;
            var content1 = Helpers.GetTransferredFileContent(fullPath);

            var options = new Options
            {
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true,
            };

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile1.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
            };

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            result = await SFTP.UploadFiles(source, destination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            var content2 = Helpers.GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1, content2);
        }

        [Test]
        public async Task UploadFiles_AppendingToExistingFileRenameBoth()
        {
            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            var fullPath = TestDestination.Directory + "/" + TestSource.FileName;
            var content1 = Helpers.GetTransferredFileContent(fullPath);

            var options = new Options
            {
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true,
            };

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile1.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
            };

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            result = await SFTP.UploadFiles(source, destination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            var content2 = Helpers.GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1, content2);
        }

        [Test]
        public async Task UploadFiles_AppendingToExistingFileRenameBothWithSourceFileNameStar()
        {
            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            var fullPath = TestDestination.Directory + "/" + TestSource.FileName;
            var content1 = Helpers.GetTransferredFileContent(fullPath);

            var options = new Options
            {
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true,
            };

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile1.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
            };

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            result = await SFTP.UploadFiles(source, destination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            var content2 = Helpers.GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1, content2);
        }
    }
}
