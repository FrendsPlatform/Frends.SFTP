using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    public class ConnectivityTests : UploadFilesTestBase
    {
        [Test]
        public async Task UploadFiles_TestWithLargerBuffer()
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

            var result = await SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken());
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

            Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        }

        [Test]
        public async Task UploadFiles_TestTransferWithMD5ServerFingerprint()
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

            var result = await SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestTransferWithSHA256ServerFingerprint()
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

            var result = await SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestPrivateKeyFileRsaFromString()
        {
            var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));
            var connection = Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
            connection.PrivateKeyPassphrase = "passphrase";
            connection.PrivateKeyString = key;

            var result = await SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestPrivateKeyFileRsa()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
            connection.PrivateKeyPassphrase = "passphrase";
            connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

            var result = await SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestKeyboardInteractiveAuthentication()
        {
            var connection = Helpers.GetSftpConnection();
            connection.UseKeyboardInteractiveAuthentication = true;

            var result = await SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestWithInteractiveKeyboardAuthenticationAndPrivateKey()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePrivateKeyFile;
            connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");
            connection.Password = null;
            connection.PrivateKeyPassphrase = "passphrase";
            connection.UseKeyboardInteractiveAuthentication = true;
            connection.PromptAndResponse = new PromptResponse[] { new PromptResponse { Prompt = "Password", Response = "pass" } };

            var result = await SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            connection.Authentication = AuthenticationType.UsernamePrivateKeyString;
            connection.PrivateKeyFile = null;
            connection.PrivateKeyString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));
            connection.PrivateKeyPassphrase = "passphrase";

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                Action = DestinationAction.Overwrite,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            result = await SFTP.UploadFiles(_source, destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestKeepAliveIntervalWithDefault()
        {
            Helpers.CopyLargeTestFile(10);

            var connection = Helpers.GetSftpConnection();

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(11, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestKeepAliveIntervalWith1ms()
        {
            Helpers.CopyLargeTestFile(10);

            var connection = Helpers.GetSftpConnection();
            connection.KeepAliveInterval = 1;
            connection.BufferSize = 256;

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(11, result.SuccessfulTransferCount);
        }
    }
}



