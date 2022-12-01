using NUnit.Framework;
using System.IO;
using System.Threading;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    class EncodingTests : UploadFilesTestBase
    {
        [Test]
        public void UploadFiles_TransferWithANSIFileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileNameEncoding = FileEncoding.ANSI
            };

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TransferWithASCIIFileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileNameEncoding = FileEncoding.ASCII
            };

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TransferWithUTF8WithoutBomFileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = false
            };

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TransferWithUTF8WithBomFileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TransferWithWin1252FileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileNameEncoding = FileEncoding.WINDOWS1252
            };

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TransferWithOtherFileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileNameEncoding = FileEncoding.Other,
                FileNameEncodingInString = "windows-1252"
            };

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TransferWithASCIIFileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileContentEncoding = FileEncoding.ASCII,
                Action = DestinationAction.Append
            };

            Helpers.UploadSingleTestFile(destination.Directory, Path.Combine(_workDir, _source.FileName));

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TransferWithANSIFileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileContentEncoding = FileEncoding.ANSI,
                Action = DestinationAction.Append
            };

            Helpers.UploadSingleTestFile(destination.Directory, Path.Combine(_workDir, _source.FileName));

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TransferWithUTF8WithBomFileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileContentEncoding = FileEncoding.UTF8,
                EnableBomForContent = true,
                Action = DestinationAction.Append
            };

            Helpers.UploadSingleTestFile(destination.Directory, Path.Combine(_workDir, _source.FileName));

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TransferWithUTF8WithoutBomFileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileContentEncoding = FileEncoding.UTF8,
                EnableBomForContent = false,
                Action = DestinationAction.Append
            };

            Helpers.UploadSingleTestFile(destination.Directory, Path.Combine(_workDir, _source.FileName));

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TransferWithWIN1252FileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileContentEncoding = FileEncoding.WINDOWS1252,
                Action = DestinationAction.Append
            };

            Helpers.UploadSingleTestFile(destination.Directory, Path.Combine(_workDir, _source.FileName));

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TransferWithOtherFileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileContentEncoding = FileEncoding.Other,
                FileContentEncodingInString = "Windows-1252",
                Action = DestinationAction.Append
            };

            Helpers.UploadSingleTestFile(destination.Directory, Path.Combine(_workDir, _source.FileName));

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }
    }
}



