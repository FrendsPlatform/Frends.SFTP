using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.UploadFiles.Definitions;
using NUnit.Framework;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    internal class TransferTests : UploadFilesTestBase
    {
        [Test]
        public async Task UploadFiles_TestSimpleTransfer()
        {
            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.AreEqual(Path.Combine(TestDestination.Directory, TestSource.FileName).Replace("\\", "/"), result.TransferredDestinationFilePaths.ToList().FirstOrDefault());
        }

        [Test]
        public async Task UploadFiles_TestThat8COFilesAreNotLeft()
        {
            await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            var files = Directory.GetFiles(TestWorkDir, "*.8CO").ToList();
            Assert.AreEqual(0, files.Count());
            files = Directory.GetFiles(TestWorkDir, "*").ToList();
            Assert.IsTrue(files.Contains(Path.Combine(TestWorkDir, TestSource.FileName)));
        }

        [Test]
        public async Task UploadFiles_TestUploadWithFileMaskEverything()
        {
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "*",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestUploadWithDefaultFileNameUploadsEverything()
        {
            var source = new Source
            {
                Directory = TestWorkDir,
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestWithOperationLogDisabled()
        {
            var options = new Options
            {
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = false,
            };

            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, options, TestInfo, CancellationToken.None);
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
                EnableBomForFileName = true,
            };

            var result = await SFTP.UploadFiles(TestSource, destination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestOneErrorInTransferWithMultipleFiles()
        {
            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
            };

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
                FileName = "*.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            result = await SFTP.UploadFiles(source, destination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.AreEqual(1, result.FailedTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestWithFileMaskWithFileAlreadyInDestination()
        {
            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "*File1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var options = new Options
            {
                ThrowErrorOnFailure = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            source.FileName = "*.txt";
            result = await SFTP.UploadFiles(source, TestDestination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FailedTransferCount);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestSingleFileTransferWithError()
        {
            var options = new Options
            {
                ThrowErrorOnFailure = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true,
            };

            var destination = new Destination
            {
                Directory = TestDestination.Directory,
                FileName = string.Empty,
                Action = DestinationAction.Error,
            };

            Helpers.UploadSingleTestFile(TestDestination.Directory, Path.Combine(TestWorkDir, "SFTPUploadTestFile1.txt"));

            var result = await SFTP.UploadFiles(TestSource, destination, TestConnection, options, TestInfo, CancellationToken.None);
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
                EnableBomForFileName = true,
            };

            Helpers.UploadSingleTestFile(destination.Directory, Path.Combine(TestWorkDir, TestSource.FileName));

            var result = await SFTP.UploadFiles(TestSource, destination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.CheckFileExistsInDestination(destination.Directory + "/" + TestSource.FileName));
        }

        [Test]
        public async Task UploadFiles_TestUploadWithOnlyRenameSourceDuringTransferEnabled()
        {
            var options = new Options
            {
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true,
            };

            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.CheckFileExistsInDestination(TestDestination.Directory + "/" + TestSource.FileName));
        }

        [Test]
        public async Task UploadFiles_TestUploadWithOnlyRenameDestinationDuringTransferEnabled()
        {
            var options = new Options
            {
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true,
            };

            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, options, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.CheckFileExistsInDestination(TestDestination.Directory + "/" + TestSource.FileName));
        }

        [Test]
        public async Task UploadFiles_NoSourceFilesAndIgnoreShouldNotThrowException()
        {
            var source = new Source
            {
                Directory = TestSource.Directory,
                FileName = "NonExistingFile.txt",
                Action = SourceAction.Ignore,
                Operation = SourceOperation.Delete,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ActionSkipped);
        }

        [Test]
        public async Task UploadFiles_NoSourceFilesAndInfoShouldNotThrowException()
        {
            var source = new Source
            {
                Directory = TestSource.Directory,
                FileName = "NonExistingFile.txt",
                Action = SourceAction.Info,
                Operation = SourceOperation.Delete,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
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
                Directory = TestSource.Directory,
                FileName = "*",
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
            };

            var connection = new Connection
            {
                ConnectionTimeout = TestConnection.ConnectionTimeout,
                Address = TestConnection.Address,
                Port = TestConnection.Port,
                Authentication = TestConnection.Authentication,
                UserName = TestConnection.UserName,
                Password = TestConnection.Password,
                BufferSize = 256,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, connection, TestOptions, TestInfo, CancellationToken.None);
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
                FilePaths = filePaths,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
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
                FilePaths = filePaths,
            };

            var options = new Options
            {
                ThrowErrorOnFailure = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, options, TestInfo, CancellationToken.None);
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
                FilePaths = filePaths,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ActionSkipped);
        }

        [Test]
        public async Task UploadFiles_TestWithFilePathsEvenIfSourceFileIsAssigned()
        {
            var filePaths = Helpers.CreateDummyFiles(3);

            var source = new Source
            {
                Directory = TestSource.Directory,
                FileName = "*.csv",
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
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
                Directory = TestSource.Directory,
                FileName = "*",
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
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
                Directory = TestSource.Directory,
                FileName = "*",
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
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
                Directory = TestSource.Directory,
                FileName = "*",
                Action = SourceAction.Info,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths,
            };

            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None));
            Assert.AreEqual($"Invalid type for parameter FilePaths. Expected array but was {filePaths.GetType()}", ex.Message);
        }

        [Test]
        public async Task UploadFiles_TestNormalTransferWithTempPath()
        {
            var temp = Path.Combine(TestWorkDir, "temp");
            Directory.CreateDirectory(temp);
            var info = new Info
            {
                WorkDir = temp,
            };

            var result = await SFTP.UploadFiles(TestSource, TestDestination, TestConnection, TestOptions, info, CancellationToken.None);
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
                FileName = string.Empty,
            };
            var result = await SFTP.UploadFiles(TestSource, destination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestPreserveDirectoryStructureWithMacro()
        {
            var localSourceDir = Path.Combine(TestWorkDir, "nestedTestSource");
            Helpers.CreateLocalFilesWithSubdirectories(localSourceDir, new Dictionary<string, List<string>>
            {
                { string.Empty,                  new List<string> { "root.txt" } },
                { "sub1",              new List<string> { "file1.txt" } },
                { Path.Combine("sub1", "sub2"),        new List<string> { "file2.txt" } },
                { Path.Combine("sub1", "sub2", "sub3"),  new List<string> { "file3.txt" } },
            });

            var source = new Source
            {
                Directory = localSourceDir,
                FileName = "*.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
                IncludeSubdirectories = true,
            };

            var destination = new Destination
            {
                Directory = "/upload/%SourceRelativeDirectory%",
                Action = DestinationAction.Error,
            };

            var result = await SFTP.UploadFiles(source, destination, TestConnection, TestOptions, TestInfo, CancellationToken.None);

            Assert.IsTrue(result.Success, "Transfer should succeed");
            Assert.AreEqual(4, result.SuccessfulTransferCount, "Should transfer 4 files");

            Assert.IsTrue(Helpers.CheckFileExistsInDestination("/upload/root.txt"));
            Assert.IsTrue(Helpers.CheckFileExistsInDestination("/upload/sub1/file1.txt"));
            Assert.IsTrue(Helpers.CheckFileExistsInDestination("/upload/sub1/sub2/file2.txt"));
            Assert.IsTrue(Helpers.CheckFileExistsInDestination("/upload/sub1/sub2/sub3/file3.txt"));
        }

        [Test]
        public async Task UploadFiles_TestRecursiveWithoutMacro()
        {
            var localSourceDir = Path.Combine(TestWorkDir, "nestedTestSource2");
            Helpers.CreateLocalFilesWithSubdirectories(localSourceDir, new Dictionary<string, List<string>>
            {
                { string.Empty,          new List<string> { "root.txt" } },
                { "sub1",      new List<string> { "file1.txt" } },
                { Path.Combine("sub1", "sub2"), new List<string> { "file2.txt" } },
            });

            var source = new Source
            {
                Directory = localSourceDir,
                FileName = "*.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
                IncludeSubdirectories = true,
            };

            var destination = new Destination
            {
                Directory = "/upload/flat",
                Action = DestinationAction.Overwrite,
            };

            var result = await SFTP.UploadFiles(source, destination, TestConnection, TestOptions, TestInfo, CancellationToken.None);

            Assert.IsTrue(result.Success, "Transfer should succeed");
            Assert.AreEqual(3, result.SuccessfulTransferCount, "Should transfer 3 files");

            Assert.IsTrue(Helpers.CheckFileExistsInDestination("/upload/flat/root.txt"));
            Assert.IsTrue(Helpers.CheckFileExistsInDestination("/upload/flat/file1.txt"));
            Assert.IsTrue(Helpers.CheckFileExistsInDestination("/upload/flat/file2.txt"));
        }

        [Test]
        public async Task UploadFiles_TestWithoutIncludeSubdirectories_OnlyRootFiles()
        {
            var localSourceDir = Path.Combine(TestWorkDir, "nestedTestSource3");
            Helpers.CreateLocalFilesWithSubdirectories(localSourceDir, new Dictionary<string, List<string>>
            {
                { string.Empty,          new List<string> { "root1.txt", "root2.txt" } },
                { "sub1",      new List<string> { "file1.txt" } },
                { Path.Combine("sub1", "sub2"), new List<string> { "file2.txt" } },
            });

            var source = new Source
            {
                Directory = localSourceDir,
                FileName = "*.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
                IncludeSubdirectories = false,
            };

            var destination = new Destination
            {
                Directory = "/upload/root_only",
                Action = DestinationAction.Error,
            };

            var result = await SFTP.UploadFiles(source, destination, TestConnection, TestOptions, TestInfo, CancellationToken.None);

            Assert.IsTrue(result.Success, "Transfer should succeed");
            Assert.AreEqual(2, result.SuccessfulTransferCount, "Should transfer ONLY 2 files from root directory");

            Assert.IsTrue(Helpers.CheckFileExistsInDestination("/upload/root_only/root1.txt"));
            Assert.IsTrue(Helpers.CheckFileExistsInDestination("/upload/root_only/root2.txt"));

            Assert.IsFalse(Helpers.CheckFileExistsInDestination("/upload/root_only/file1.txt"));
            Assert.IsFalse(Helpers.CheckFileExistsInDestination("/upload/root_only/file2.txt"));
        }

        [Test]
        public async Task UploadFiles_TestWithFilePathsAndNullDirectory()
        {
            var files = Helpers.CreateDummyFiles(3);

            var filePaths = new string[] { files[0], files[1] };

            var source = new Source
            {
                Directory = null, // Directory is null
                FileName = string.Empty,
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
                FilePaths = filePaths,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(2, result.SuccessfulTransferCount);
        }
    }
}