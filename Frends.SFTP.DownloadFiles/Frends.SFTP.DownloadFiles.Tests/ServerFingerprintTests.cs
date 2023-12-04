using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    [TestFixture]
    public class ServerFingerprintTests : DownloadFilesTestBase
    {
        internal static string _MD5;
        internal static string _Sha256Hex;
        internal static string _Sha256Hash;

        [OneTimeSetUp]
        public override void OneTimeSetup()
        {
            base.OneTimeSetup();
            var (fingerPrint, hostKey) = Helpers.GetServerFingerPrintAndHostKey();
            _MD5 = Helpers.ConvertToMD5Hex(fingerPrint);
            _Sha256Hex = Helpers.ConvertToSHA256Hex(hostKey);
            _Sha256Hash = Helpers.ConvertToSHA256Hash(hostKey);
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Helpers.UploadTestFiles(_source.Directory, 3);
        }

        [Test]
        public async Task DownloadFiles_TestTransferWithExpectedServerFingerprintAsHexSha256()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = _Sha256Hex;
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

            var result = await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestTransferWithExpectedServerFingerprintAsHexSha256WithAltercations()
        {
            var connection = Helpers.GetSftpConnection();
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;
            connection.ServerFingerPrint = _Sha256Hash.Replace("=", "");
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

            var result = await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestTransferWithExpectedServerFingerprintAsSha256()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = _Sha256Hash;
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

            var result = await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestTransferWithExpectedServerFingerprintAsMD5()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = _MD5;
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

            var result = await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestTransferWithExpectedServerFingerprintAsMD5ToLower()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = _MD5.ToLower();
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

            var result = await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public async Task DownloadFiles_TestTransferWithExpectedServerFingerprintAsMD5Hash()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = _MD5.Replace(":", "");
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

            var result = await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ActionSkipped);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void DownloadFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Error when establishing connection to the Server: Key exchange negotiation failed.."));
        }

        [Test]
        public void DownloadFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Error when establishing connection to the Server: Key exchange negotiation failed.."));
        }

        [Test]
        public void DownloadFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Error when establishing connection to the Server: Key exchange negotiation failed.."));
        }

        [Test]
        public void DownloadFiles_TestThrowsTransferWithInvalidExpectedServerFingerprint()
        {
            var connection = Helpers.GetSftpConnection();
            connection.ServerFingerPrint = "nuDEsWN4tfEQ684x7RySiCwjGXmX2CfBaBHeSqO8vfiurenvire56";
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Error when establishing connection to the Server: Key exchange negotiation failed.."));
            Assert.IsTrue(ex.Message.Contains("Expected server fingerprint was given in unsupported format."));
        }

        [Test]
        public void DownloadFiles_TestShouldThrowWithoutPromptAndResponse()
        {
            var connection = Helpers.GetSftpConnection();
            connection.Authentication = AuthenticationType.UsernamePrivateKeyFile;
            connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");
            connection.Password = null;
            connection.PrivateKeyPassphrase = "passphrase";
            connection.UseKeyboardInteractiveAuthentication = true;
            connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;
            connection.ServerFingerPrint = _Sha256Hash.Replace("=", "");

            var ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Failure in Keyboard-interactive authentication: No response given for server prompt request --> Password"));

            connection.Authentication = AuthenticationType.UsernamePrivateKeyString;
            connection.PrivateKeyFile = null;
            connection.PrivateKeyString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));
            connection.PrivateKeyPassphrase = "passphrase";

            var destination = new Destination
            {
                Directory = Path.Combine(_workDir, "destination"),
                Action = DestinationAction.Overwrite,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            ex = Assert.ThrowsAsync<Exception>(async () => await SFTP.DownloadFiles(_source, destination, connection, _options, _info, new CancellationToken()));
            Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Failure in Keyboard-interactive authentication: No response given for server prompt request --> Password"));
        }
    }
}



