using NUnit.Framework;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    [TestFixture]
    class EncodingTests : DownloadFilesTestBase
    {
        [SetUp]
        public void IndividualSetup()
        {
            Helpers.UploadTestFiles(_source.Directory, 3);
        }

        [Test]
        public async Task UploadFiles_TransferWithANSIFileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory
            };

            var connection = Helpers.GetSftpConnection();
            connection.FileNameEncoding = FileEncoding.ANSI;

            var result = await SFTP.DownloadFiles(_source, destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TransferWithASCIIFileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory
            };

            var connection = Helpers.GetSftpConnection();
            connection.FileNameEncoding = FileEncoding.ASCII;

            var result = await SFTP.DownloadFiles(_source, destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TransferWithUTF8WithoutBomFileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory
            };

            var connection = Helpers.GetSftpConnection();
            connection.EnableBomForFileName = false;

            var result = await SFTP.DownloadFiles(_source, destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TransferWithUTF8WithBomFileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory,
            };

            var result = await SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TransferWithWin1252FileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory
            };

            var connection = Helpers.GetSftpConnection();
            connection.FileNameEncoding = FileEncoding.WINDOWS1252;

            var result = await SFTP.DownloadFiles(_source, destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TransferWithOtherFileNameEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory
            };

            var connection = Helpers.GetSftpConnection();
            connection.FileNameEncoding = FileEncoding.Other;
            connection.FileNameEncodingInString = "windows-1252";

            var result = await SFTP.DownloadFiles(_source, destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TransferWithASCIIFileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory,
                FileContentEncoding = FileEncoding.ASCII,
                Action = DestinationAction.Append
            };

            Directory.CreateDirectory(destination.Directory);
            File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

            var result = await SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TransferWithANSIFileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory,
                FileContentEncoding = FileEncoding.ANSI,
                Action = DestinationAction.Append
            };

            Directory.CreateDirectory(destination.Directory);
            File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

            var result = await SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TransferWithUTF8WithBomFileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory,
                FileContentEncoding = FileEncoding.UTF8,
                EnableBomForContent = true,
                Action = DestinationAction.Append
            };

            Directory.CreateDirectory(destination.Directory);
            File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

            var result = await SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TransferWithUTF8WithoutBomFileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory,
                FileContentEncoding = FileEncoding.UTF8,
                EnableBomForContent = false,
                Action = DestinationAction.Append
            };

            Directory.CreateDirectory(destination.Directory);
            File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

            var result = await SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TransferWithWIN1252FileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory,
                FileContentEncoding = FileEncoding.WINDOWS1252,
                Action = DestinationAction.Append
            };

            Directory.CreateDirectory(destination.Directory);
            File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

            var result = await SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task UploadFiles_TransferWithOtherFileContentEncoding()
        {
            var destination = new Destination
            {
                Directory = _destination.Directory,
                FileContentEncoding = FileEncoding.Other,
                FileContentEncodingInString = "Windows-1252",
                Action = DestinationAction.Append
            };

            Directory.CreateDirectory(destination.Directory);
            File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

            var result = await SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }
    }
}



