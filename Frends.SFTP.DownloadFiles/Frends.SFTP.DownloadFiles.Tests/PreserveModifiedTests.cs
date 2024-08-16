using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    [TestFixture]
    class PreserveModifiedTests : DownloadFilesTestBase
    {

        private Destination destination;
        private Options options;
        private string sourcePath;
        private DateTime date;

        [SetUp]
        public void SetUp()
        {
            destination = new Destination
            {
                Directory = Path.Combine(_workDir, "destination"),
                Action = DestinationAction.Overwrite
            };

            options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            sourcePath = Path.Combine(_workDir, _source.FileName);
            date = File.GetLastWriteTime(sourcePath);
            Helpers.SetTestFileLastModified(_source.Directory + "/" + _source.FileName, date);
        }

        [Test]
        public async Task DownloadFiles_TestPreserveLastModifiedWithoutRename()
        {
            var result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            var destFilePath = Path.Combine(destination.Directory, _source.FileName);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(destFilePath));
            Assert.AreEqual(date.ToString(), File.GetLastWriteTime(destFilePath).ToString());
        }

        [Test]
        public async Task DownloadFiles_TestPreserveLastModifiedWithRename()
        {
            options.RenameSourceFileBeforeTransfer = true;
            options.RenameDestinationFileDuringTransfer = true;

            var result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            var destFilePath = Path.Combine(destination.Directory, _source.FileName);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(destFilePath));
            Assert.AreEqual(date.ToString(), File.GetLastWriteTime(destFilePath).ToString());
        }

        [Test]
        public async Task DownloadFiles_TestPreserveLastModifiedWithSourceRename()
        {
            options.RenameSourceFileBeforeTransfer = true;
            options.RenameDestinationFileDuringTransfer = false;

            var result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            var destFilePath = Path.Combine(destination.Directory, _source.FileName);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(destFilePath));
            Assert.AreEqual(date.ToString(), File.GetLastWriteTime(destFilePath).ToString());
        }

        [Test]
        public async Task DownloadFiles_TestPreserveLastModifiedWithDestinationRename()
        {
            options.RenameSourceFileBeforeTransfer = false;
            options.RenameDestinationFileDuringTransfer = true;

            var result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            var destFilePath = Path.Combine(destination.Directory, _source.FileName);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(destFilePath));
            Assert.AreEqual(date.ToString(), File.GetLastWriteTime(destFilePath).ToString());
        }

        [Test]
        public async Task DownloadFiles_TestPreserveLastModifiedWithRenameAndDeleteSourceFile()
        {
            options.RenameSourceFileBeforeTransfer = true;
            options.RenameDestinationFileDuringTransfer = true;

            var source = new Source
            {
                Directory = "/upload/Upload",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Delete,
            };

            var result = await SFTP.DownloadFiles(source, destination, _connection, options, _info, new CancellationToken());
            var destFilePath = Path.Combine(destination.Directory, _source.FileName);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            Assert.IsTrue(File.Exists(destFilePath));
            Assert.AreEqual(date.ToString(), File.GetLastWriteTime(destFilePath).ToString());
        }
    }

}


