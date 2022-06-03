using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    [TestFixture]
    public class ConnectivityTests : DownloadFilesTestBase
    {
        [Test]
        public void DownloadFiles_TestWithLargerBuffer()
        {
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, "LargeTestFile.bin") }, _source.Directory);

            var connection = Helpers.GetSftpConnection();
            connection.BufferSize = 256;

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "LargeTestFile.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = SFTP.DownloadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void DownloadFiles_TestTransferThatThrowsWithIncorrectCredentials()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ConnectionTimeout = 10;
            connection.UserName = "demo";
            connection.Password = "demo";

            var result = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.That(result.Message.StartsWith("SFTP transfer failed: Authentication of SSH session failed: Permission denied (password)"));
        }

        [Test]
        public void DownloadFiles_TestTransferWithMD5ServerFingerprint()
        {
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = Helpers.GetServerFingerprintAsMD5String();

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var result = SFTP.DownloadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void DownloadFiles_TestTransferWithSHA256ServerFingerprint()
        {
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = Helpers.GetServerFingerprintAsSHA256String();

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = _source.FileName,
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var result = SFTP.DownloadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void DownloadFiles_TestThrowsWithWrongPort()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Port = 51651;

            var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.That(ex.Message.StartsWith("SFTP transfer failed: Unable to establish the socket: No such host is known"));
        }

        [Test]
        public void DownloadFiles_TestPrivateKeyFileRsa()
        {
            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

            var connection = Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
            connection.PrivateKeyFilePassphrase = "passphrase";
            connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

            var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void DownloadFiles_TestPrivateKeyFileRsaFromString()
        {
            var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

            Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

            var connection = Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
            connection.PrivateKeyFilePassphrase = "passphrase";
            connection.PrivateKeyString = key;

            var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }
    }
}
