using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    [TestFixture]
    public class ConnectivityTests : DownloadFilesTestBase
    {
        private readonly string invalidPwd = "demo";

        [Test]
        public async Task DownloadFiles_TestWithLargerBuffer()
        {
            Helpers.UploadLargeTestFiles(_source.Directory, 1);

            var connection = Helpers.GetSftpConnection();
            connection.KeepAliveInterval = 10;
            connection.BufferSize = 256;

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "LargeTestFile1.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.DownloadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void DownloadFiles_TestTransferThatThrowsWithIncorrectCredentials()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ConnectionTimeout = 10;
            connection.UserName = invalidPwd;
            connection.Password = invalidPwd;

            var result = Assert.ThrowsAsync<Exception>(async () => await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(result.Message.StartsWith("SFTP transfer failed: Authentication of SSH session failed: Permission denied (password)"));
        }

        [Test]
        public async Task DownloadFiles_TestPrivateKeyFileRsa()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
            connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

            var result = await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_TestPrivateKeyFileRsaFromString()
        {
            var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

            var connection = Helpers.GetSftpConnection();
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;
            connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
            connection.PrivateKeyString = key;

            var result = await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_TestWithInteractiveKeyboardAuthentication()
        {
            var connection = Helpers.GetSftpConnection();
            connection.UseKeyboardInteractiveAuthentication = true;

            var result = await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_TestWithInteractiveKeyboardAuthenticationAndPrivateKey()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePrivateKeyFile;
            connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");
            connection.Password = null;
            connection.UseKeyboardInteractiveAuthentication = true;
            connection.PromptAndResponse = new PromptResponse[] { new PromptResponse { Prompt = "Password", Response = "pass" } };

            var result = await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            connection.Authentication = AuthenticationType.UsernamePrivateKeyString;
            connection.PrivateKeyFile = null;
            connection.PrivateKeyString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

            var destination = new Destination
            {
                Directory = Path.Combine(_workDir, "destination"),
                Action = DestinationAction.Overwrite
            };

            result = await SFTP.DownloadFiles(_source, destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(File.Exists(Path.Combine(_destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_TestKeepAliveIntervalWithDefault()
        {
            Helpers.UploadLargeTestFiles(_source.Directory, 1);

            var connection = Helpers.GetSftpConnection();

            var source = new Source
            {
                Directory = _source.Directory,
                FileName = "*.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.DownloadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestKeepAliveIntervalWith1ms()
        {
            Helpers.UploadLargeTestFiles(_source.Directory, 1);

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

            var result = await SFTP.DownloadFiles(source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }
    }
}


