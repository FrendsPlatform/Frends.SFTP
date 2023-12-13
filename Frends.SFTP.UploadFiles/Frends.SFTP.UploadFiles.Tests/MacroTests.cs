using NUnit.Framework;
using System.IO;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    class MacroTests : UploadFilesTestBase
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
                EnableBomForFileName = true
            };

            var result = await SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var date = DateTime.Now;
            var file = "/upload/Upload/SFTPUploadTestFile1" + date.ToString(@"yyyy-MM-dd") + ".txt";
            Assert.AreEqual(file, result.TransferredDestinationFilePaths.ToList().FirstOrDefault());
            Assert.IsTrue(Helpers.CheckFileExistsInDestination(file));
        }

        [Test]
        public async Task UploadFiles_TestSourceDirectoryWithMacros()
        {
            var year = DateTime.Now.Year.ToString();
            var path = Path.Combine(_workDir, "testfolder_" + year);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(path, _source.FileName), true);
            File.Copy(Path.Combine(_workDir, "SFTPUploadTestFile2.txt"), Path.Combine(path, "SFTPUploadTestFile2.txt"), true);

            var source = new Source
            {
                Directory = Path.Combine(_workDir, "testfolder_%Year%"),
                FileName = "*.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Delete
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
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
                FileName = "",
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var result = await SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            var year = DateTime.Now.Year.ToString();
            Assert.IsTrue(Helpers.CheckFileExistsInDestination("upload/Upload/test" + year));
        }

        [Test]
        public async Task UploadFiles_TestSourceFileMoveWithMacros()
        {
            var year = DateTime.Now.Year.ToString();
            var to = Path.Combine(_workDir, year + "_uploaded");
            Directory.CreateDirectory(to);
            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = Path.Combine(_workDir, "%Year%_uploaded")
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
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
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%%SourceFileExtension%"
            };

            var result = await SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(Path.Combine(_workDir, "uploaded_" + source.FileName)));
        }
    }
}



