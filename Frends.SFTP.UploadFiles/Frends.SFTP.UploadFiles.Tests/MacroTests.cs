using Frends.SFTP.UploadFiles.Definitions;
using Frends.SFTP.UploadFiles.Tests.Lib;
using NUnit.Framework;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    internal class MacroTests : UploadFilesTestBase
    {
        [Test]
        public async Task UploadFiles_TestUsingMacros()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "%SourceFileName%%Date%%SourceFileExtension%",
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
            };

            var result = await SFTP.UploadFiles(TestSource, destination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            var date = DateTime.Now;
            var file = "/upload/Upload/SFTPUploadTestFile1" + date.ToString(@"yyyy-MM-dd") + ".txt";
            Assert.AreEqual(file, result.TransferredDestinationFilePaths.ToList().FirstOrDefault());
            Assert.IsTrue(Lib.Helpers.CheckFileExistsInDestination(file));
        }

        [Test]
        public async Task UploadFiles_TestSourceDirectoryWithMacros()
        {
            var year = DateTime.Now.Year.ToString();
            var path = Path.Combine(TestWorkDir, "testfolder_" + year);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.Copy(Path.Combine(TestWorkDir, TestSource.FileName), Path.Combine(path, TestSource.FileName), true);
            File.Copy(Path.Combine(TestWorkDir, "SFTPUploadTestFile2.txt"), Path.Combine(path, "SFTPUploadTestFile2.txt"), true);

            var source = new Source
            {
                Directory = Path.Combine(TestWorkDir, "testfolder_%Year%"),
                FileName = "*.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Delete,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.SuccessfulTransferCount);

            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        [Test]
        public async Task UploadFiles_TestDestinationDirectoryWithMacros()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload/test%Year%",
                FileName = string.Empty,
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
            };

            var result = await SFTP.UploadFiles(TestSource, destination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            var year = DateTime.Now.Year.ToString();
            Assert.IsTrue(Lib.Helpers.CheckFileExistsInDestination("upload/Upload/test" + year));
        }

        [Test]
        public async Task UploadFiles_TestSourceFileMoveWithMacros()
        {
            var year = DateTime.Now.Year.ToString();
            var to = Path.Combine(TestWorkDir, year + "_uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = TestSource.Directory,
                FileName = TestSource.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = Path.Combine(TestWorkDir, "%Year%_uploaded"),
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(to, source.FileName)));
            Directory.Delete(to, true);
        }

        [Test]
        public async Task UploadFiles_TestSourceFileRenameWithMacros()
        {
            var source = new Source
            {
                Directory = TestSource.Directory,
                FileName = TestSource.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%%SourceFileExtension%",
            };

            var result = await SFTP.UploadFiles(source, TestDestination, TestConnection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(TestWorkDir, "uploaded_" + source.FileName)));
        }
    }
}
