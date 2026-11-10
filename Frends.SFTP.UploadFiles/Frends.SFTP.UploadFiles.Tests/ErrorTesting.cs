using Frends.SFTP.UploadFiles.Definitions;
using NUnit.Framework;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    internal class ErrorTesting : UploadFilesTestBase
    {
        [Test]
        public void UploadFiles_TestTransferThatThrowsIfFileNotExist()
        {
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "FileThatDontExist.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, default));
            Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed:"));
        }

        [Test]
        public async Task UploadFiles_TestTransferThatExistsThrowsError()
        {
            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, TestInfo, default);
            Assert.IsTrue(result.Success);

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, TestInfo, default));
            Assert.IsTrue(ex.Message.StartsWith($"SFTP transfer failed: 1 Errors: Failure in CheckIfDestination"));
        }

        [Test]
        public void UploadFiles_TestThrowsWithWrongPort()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Port = 51651;

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(TestSource, TestDestination, connection, TestOptions, TestInfo, default));
            Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Unable to establish the socket:"));
        }

        [Test]
        public void UploadFiles_TestThrowsWithWrongAddress()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Address = "local";

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(TestSource, TestDestination, connection, TestOptions, TestInfo, default));
            Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Unable to establish the socket:"));
        }

        [Test]
        public void UploadFiles_TestThrowsMovedSourceFileIsRestored()
        {
            Helpers.UploadSingleTestFile(TestDestination.Directory, Path.Combine(TestWorkDir, TestSource.FileName));

            var connection = Helpers.GetSftpConnection();
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = Path.Combine(TestWorkDir, "moved"),
            };
            Directory.CreateDirectory(source.DirectoryToMoveAfterTransfer);

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(source, TestDestination, connection, TestOptions, TestInfo, default));
            Assert.IsTrue(ex.Message.StartsWith($"SFTP transfer failed: 1 Errors: Failure in CheckIfDestination"));
            Assert.IsTrue(File.Exists(Path.Combine(TestSource.Directory, TestSource.FileName)));

            Directory.Delete(source.DirectoryToMoveAfterTransfer, true);
        }

        [Test]
        public void UploadFiles_TestSourceMoveWithFileAlreadyInMovedFolder()
        {
            var connection = Helpers.GetSftpConnection();
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = Path.Combine(TestWorkDir, "moved"),
            };
            Directory.CreateDirectory(source.DirectoryToMoveAfterTransfer);
            File.Copy(Path.Combine(source.Directory, source.FileName), Path.Combine(source.DirectoryToMoveAfterTransfer, source.FileName));

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(source, TestDestination, connection, TestOptions, TestInfo, default));
            var test = ex.Message;
            Assert.IsTrue(ex.Message.Contains($"Error: Failure in source operation:"));
            Assert.IsTrue(File.Exists(Path.Combine(TestSource.Directory, TestSource.FileName)));

            Directory.Delete(source.DirectoryToMoveAfterTransfer, true);
        }

        [Test]
        public void UploadFiles_TestErrorMessage()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Password = "cuinbeu8i9ch";

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(TestSource, TestDestination, connection, TestOptions, TestInfo, default));
            Assert.IsTrue(ex.Message.Contains($@"FRENDS SFTP file transfer '' from 'FILE://{TestSource.Directory}/{TestSource.FileName}' to 'SFTP://{connection.Address}/{TestDestination.Directory}':"));
        }

        [Test]
        public async Task UploadFiles_TestDontThrowWhenFilesInFilePathsAreNotFound()
        {
            var filePaths = Helpers.CreateDummyFiles(3);
            foreach (var file in filePaths)
            {
                File.Delete(file.ToString());
            }

            var source = new Source
            {
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, default);
            Assert.IsTrue(result.ActionSkipped);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void UploadFiles_TestThrowsWhenFilesInFilePathsAreNotFound()
        {
            var filePaths = Helpers.CreateDummyFiles(3);
            foreach (var file in filePaths)
            {
                File.Delete(file.ToString());
            }

            var source = new Source
            {
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths,
            };

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, default));
            Assert.IsTrue(ex.Message.Contains("No source files found from FilePaths"));
        }

        [Test]
        public void UploadFiles_TestTransferThatThrowsWhenFileIsLocked()
        {
            using (var stream = File.Open(Path.Combine(TestWorkDir, "SFTPUploadTestFile1.txt"), FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, TestInfo, default));
                Assert.IsFalse(ex.Message.Contains("Could not restore original source file"));
            }
        }

        [Test]
        public void UploadFiles_TestCancellationToken()
        {
            var connection = Helpers.GetSftpConnection();
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "LargeTestFile.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(source, TestDestination, connection, TestOptions, TestInfo, new CancellationTokenSource(1000).Token));
            Assert.IsTrue(ex.Message.Contains("No files transferred."));
        }

        [Test]
        public void UploadFiles_TestTimeout()
        {
            var connection = Helpers.GetSftpConnection();
            var options = new Options
            {
                Timeout = 1,
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = false,
            };

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "LargeTestFile.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var temp = Path.Combine(TestWorkDir, "temp");
            Directory.CreateDirectory(temp);
            var info = new Info
            {
                WorkDir = temp,
            };

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(source, TestDestination, connection, options, info, default));
            Console.WriteLine(ex.Message);
            Assert.IsTrue(ex.Message.Contains("No files transferred."));
            Assert.AreEqual(0, Directory.GetFiles(temp).Length);
            Assert.AreEqual(0, Directory.GetFiles(TestWorkDir).Where(e => Path.GetExtension(e) == ".8CO").Select(e => e).ToList().Count);
        }

        [Test]
        public void UploadFiles_TestTimeoutCheckTempFiles()
        {
            var connection = Helpers.GetSftpConnection();
            var options = new Options
            {
                Timeout = 1,
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = false,
            };

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "LargeTestFile.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var temp = Path.Combine(TestWorkDir, "temp");
            Directory.CreateDirectory(temp);
            var info = new Info
            {
                WorkDir = temp,
            };

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(source, TestDestination, connection, options, info, default));
            Assert.IsTrue(ex.Message.Contains("No files transferred."));
            Assert.AreEqual(0, Directory.GetFiles(temp).Length);
        }
    }
}
