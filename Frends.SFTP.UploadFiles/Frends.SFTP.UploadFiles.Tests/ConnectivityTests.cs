using Frends.SFTP.UploadFiles.Definitions;
using Frends.SFTP.UploadFiles.Tests.Lib;
using NUnit.Framework;

namespace Frends.SFTP.UploadFiles.Tests
{
    [TestFixture]
    public class ConnectivityTests : UploadFilesTestBase
    {
        [Test]
        public async Task UploadFiles_TestWithLargerBuffer()
        {
            var connection = Lib.Helpers.GetSftpConnection();
            connection.BufferSize = 256;

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "LargeTestFile.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, connection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TestTransferThatThrowsWithIncorrectCredentials()
        {
            var connection = Lib.Helpers.GetSftpConnection();
            connection.ConnectionTimeout = 10;
            connection.UserName = "demo";
            connection.Password = "demo";

            Assert.ThrowsAsync<Exception>(async () => await SFTP.UploadFiles(TestSource, TestDestination, connection, TestOptions, TestInfo, CancellationToken.None));
        }

        [Test]
        public async Task UploadFiles_TestTransferWithMD5ServerFingerprint()
        {
            var connection = Lib.Helpers.GetSftpConnection();
            connection.ServerFingerPrint = Lib.Helpers.GetServerFingerprintAsMD5String();

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, connection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestTransferWithSHA256ServerFingerprint()
        {
            var connection = Lib.Helpers.GetSftpConnection();
            connection.ServerFingerPrint = Lib.Helpers.GetServerFingerprintAsSHA256String();

            var source = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, connection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestPrivateKeyFileRsaFromString()
        {
            var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));
            var connection = Lib.Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
            connection.PrivateKeyPassphrase = "passphrase";
            connection.PrivateKeyString = key;

            var result = await SFTP.UploadFiles(TestSource, TestDestination, connection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestPrivateKeyFileRsa()
        {
            var connection = Lib.Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
            connection.PrivateKeyPassphrase = "passphrase";
            connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

            var result = await SFTP.UploadFiles(TestSource, TestDestination, connection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestKeyboardInteractiveAuthentication()
        {
            var connection = Lib.Helpers.GetSftpConnection();
            connection.UseKeyboardInteractiveAuthentication = true;

            var result = await SFTP.UploadFiles(TestSource, TestDestination, connection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task UploadFiles_TestWithInteractiveKeyboardAuthenticationAndPrivateKey()
        {
            var connection = Lib.Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePrivateKeyFile;
            connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");
            connection.Password = null;
            connection.PrivateKeyPassphrase = "passphrase";
            connection.UseKeyboardInteractiveAuthentication = true;
            connection.PromptAndResponse = new PromptResponse[] { new PromptResponse { Prompt = "Password", Response = "pass" } };

            var result = await SFTP.UploadFiles(TestSource, TestDestination, connection, TestOptions, TestInfo, CancellationToken.None);
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
                EnableBomForFileName = true,
            };

            result = await SFTP.UploadFiles(TestSource, destination, connection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestKeepAliveIntervalWithDefault()
        {
            Lib.Helpers.CopyLargeTestFile(10);

            var connection = Lib.Helpers.GetSftpConnection();

            var source = new Source
            {
                Directory = TestSource.Directory,
                FileName = "*.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, connection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(11, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestKeepAliveIntervalWith1ms()
        {
            Lib.Helpers.CopyLargeTestFile(10);

            var connection = Lib.Helpers.GetSftpConnection();
            connection.KeepAliveInterval = 1;
            connection.BufferSize = 256;

            var source = new Source
            {
                Directory = TestSource.Directory,
                FileName = "*.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.UploadFiles(source, TestDestination, connection, TestOptions, TestInfo, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(11, result.SuccessfulTransferCount);
        }
    }
}
