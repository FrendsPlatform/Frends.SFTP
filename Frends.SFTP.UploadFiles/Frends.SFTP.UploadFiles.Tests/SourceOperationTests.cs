using Frends.SFTP.UploadFiles.Definitions;
using Frends.SFTP.UploadFiles.Tests.Lib;
using NUnit.Framework;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    internal class SourceOperationTests : UploadFilesTestBase
    {
        [Test]
        public async Task UploadFiles_TestSourceOperationWithMove()
        {
            var to = Path.Combine(TestWorkDir, "uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationWithRename()
        {
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%.txt",
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(TestWorkDir, "uploaded_SFTPUploadTestFile1.txt")));
        }

        [Test]
        public async Task UploadFiles_SourceOperationNothingWithRenamingDisable()
        {
            var options = new Options
            {
                ThrowErrorOnFailure = false,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true,
            };
            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);

            Assert.IsTrue(File.Exists(Path.Combine(TestWorkDir, TestSource.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationMoveWithRenameFilesDuringTransferDisabled()
        {
            var to = Path.Combine(TestWorkDir, "destination");
            Directory.CreateDirectory(to);
            var options = new Options
            {
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true,
            };

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationRenameWithRenameFilesDuringTransferDisabled()
        {
            var options = new Options
            {
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true,
            };

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%%SourceFileExtension%",
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(TestWorkDir, "uploaded_" + source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationMoveWithRenameSourceAndDestinationFilesEnabled()
        {
            var to = Path.Combine(TestWorkDir, "destination");
            Directory.CreateDirectory(to);

            var options = new Options
            {
                ThrowErrorOnFailure = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true,
            };

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationRenameWithRenameFilesDuringTransferWithRenameSourceAndDestinationFilesEnabled()
        {
            var options = new Options
            {
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true,
            };

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%%SourceFileExtension%",
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(TestWorkDir, "uploaded_" + source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestWithSourceMoveToNonExistingDirectoryShouldReturnUnsuccessfulTransfer()
        {
            var options = new Options
            {
                ThrowErrorOnFailure = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true,
            };

            var source = new Source
            {
                Directory = TestSource.Directory,
                FileName = TestSource.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = Path.Combine(TestWorkDir, "uploaded"),
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsFalse(result.Success);

            Assert.IsTrue(File.Exists(Path.Combine(TestWorkDir, TestSource.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationWithRenameWithDifferentDirectory()
        {
            var to = Path.Combine(TestWorkDir, "uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = Path.Combine(to, "uploaded_%SourceFileName%.txt"),
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, "uploaded_SFTPUploadTestFile1.txt")));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationWithRenameWithDifferentDirectoryWithMacrosInDirectory()
        {
            var year = DateTime.Now.Year.ToString();
            var to = Path.Combine(TestWorkDir, $"{year}_uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = Path.Combine(TestWorkDir + "%Year%_uploaded", "uploaded_%SourceFileName%.txt"),
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, "uploaded_SFTPUploadTestFile1.txt")));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationWithRenameWithDifferentDirectoryWithMacrosInFileName()
        {
            var year = DateTime.Now.Year.ToString();
            var to = Path.Combine(TestWorkDir, $"{year}_uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = Path.Combine(to, "uploaded_%SourceFileName%.txt"),
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, "uploaded_SFTPUploadTestFile1.txt")));
        }

        [Test]
        public async Task UploadFiles_TestSourceOperationMoveThat8COFilesAreNotLeft()
        {
            var to = Path.Combine(TestWorkDir, "uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            var files = Directory.GetFiles(to, "*.8CO").ToList();
            Assert.AreEqual(0, files.Count());
            files = Directory.GetFiles(to, "*").ToList();
            Assert.IsTrue(files.Contains(Path.Combine(to, TestSource.FileName)));
        }

        [Test]
        public void UploadFiles_TestSourceOperationMoveFailedAnd8COFilesAreNotLeft()
        {
            var to = Path.Combine(TestWorkDir, "uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = to,
            };

            File.Copy(Path.Combine(TestWorkDir, source.FileName), Path.Combine(to, source.FileName));

            Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None));

            var files = Directory.GetFiles(TestWorkDir, "frends_*").ToList();
            Assert.AreEqual(0, files.Count());
            files = Directory.GetFiles(TestWorkDir, "*").ToList();
            Assert.IsTrue(files.Contains(Path.Combine(TestWorkDir, TestSource.FileName)));
        }
    }
}
