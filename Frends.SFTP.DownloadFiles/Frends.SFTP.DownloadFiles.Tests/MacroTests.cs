using NUnit.Framework;
using System.IO;
using System;
using System.Threading;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    [TestFixture]
    internal class MacroTests : DownloadFilesTestBase
    {
        [Test]
        public void DownloadFiles_TestUsingMacros()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory,
                FileName = "%SourceFileName%%Date%%SourceFileExtension%",
                Action = DestinationAction.Error,
            };

            var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var date = DateTime.Now;
            Assert.IsTrue(File.Exists(Path.Combine(_destWorkDir, "SFTPDownloadTestFile1" + date.ToString(@"yyyy-MM-dd") + ".txt")));
        }

        [Test]
        public void DownloadFiles_TestSourceDirectoryWithMacros()
        {
            Helpers.UploadTestFiles(_source.Directory + "/testfolder_2022", 3);

            var source = new Source
            {
                Directory = "/upload/Upload/testfolder_%Year%",
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

        }

        [Test]
        public void DownloadFiles_TestDestinationDirectoryWithMacros()
        {
            var year = DateTime.Now.Year.ToString();
            var destination = new Destination
            {
                Directory = Path.Combine(_destWorkDir, "test%Year%"),
                FileName = "",
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
                Action = DestinationAction.Error,
            };

            var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            var dir = Path.Combine(_destWorkDir, "test" + year);

            var exists = File.Exists(Path.Combine(dir, _source.FileName));
            Assert.IsTrue(exists);
        }

        [Test]
        public void DownloadFiles_TestSourceFileRenameWithMacros()
        {
            var year = DateTime.Now.Year.ToString();
            var to = "/upload/Upload/" + year + "_uploaded/" + Path.GetFileNameWithoutExtension(_source.FileName) + year + Path.GetExtension(_source.FileName);
            Helpers.CreateSubDirectory(Path.GetDirectoryName(to).Replace("\\", "/"));

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = to,
            };

            var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.SourceFileExists(to));
        }

        [Test]
        public void DownloadFiles_TestSourceFileRenameWithMacros2()
        {
            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Rename,
                FileNameAfterTransfer = "uploaded_%SourceFileName%%SourceFileExtension%"
            };

            var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.SourceFileExists(_source.Directory + "/uploaded_" + source.FileName));
        }

        [Test]
        public void DownloadFiles_TestSourceFileMoveWithMacros()
        {
            var year = DateTime.Now.Year.ToString();
            var to = $"/upload/Upload/{year}_uploaded";
            Helpers.UploadTestFiles(_source.Directory, 1, to);

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Move,
                DirectoryToMoveAfterTransfer = "/upload/Upload/%Year%_uploaded"
            };

            var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(Helpers.SourceFileExists(Path.Combine(to, _source.FileName).Replace("\\", "/")));
        }
    }
}



