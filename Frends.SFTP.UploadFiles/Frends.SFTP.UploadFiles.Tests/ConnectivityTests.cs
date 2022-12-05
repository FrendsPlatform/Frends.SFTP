using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    public class ConnectivityTests : UploadFilesTestBase
    {
        [Test]
        public void UploadFiles_TestWithLargerBuffer()
        {
            var connection = Helpers.GetSftpConnection();
            connection.BufferSize = 256;

            var source = new Source
            {
                Directory = _workDir,
                FileName = "LargeTestFile.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TestTransferThatThrowsWithIncorrectCredentials()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ConnectionTimeout = 10;
            connection.UserName = "demo";
            connection.Password = "demo";

            Assert.Throws<Exception>(() => SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        }

        [Test]
        public void UploadFiles_TestTransferWithMD5ServerFingerprint()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = Helpers.GetServerFingerprintAsMD5String();

            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var result = SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TestTransferWithSHA256ServerFingerprint()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = Helpers.GetServerFingerprintAsSHA256String();

            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var result = SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TestPrivateKeyFileRsaFromString()
        {
            var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));
            var connection = Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
            connection.PrivateKeyFilePassphrase = "passphrase";
            connection.PrivateKeyString = key;

            var result = SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TestPrivateKeyFileRsa()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
            connection.PrivateKeyFilePassphrase = "passphrase";
            connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

            var result = SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TestKeyboardInteractiveAuthentication()
        {
            var connection = Helpers.GetSftpConnection();
            connection.UseKeyboardInteractiveAuthentication = true;

            var result = SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }
    }
}



