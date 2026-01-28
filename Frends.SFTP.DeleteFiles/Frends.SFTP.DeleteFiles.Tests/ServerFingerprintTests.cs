namespace Frends.SFTP.DeleteFiles.Tests;

using System;
using System.IO;
using System.Threading.Tasks;
using Frends.SFTP.DeleteFiles.Enums;
using NUnit.Framework;
using Renci.SshNet.Common;

[TestFixture]
public class ServerFingerprintTests : UnitTestBase
{
    internal static string _MD5;
    internal static string _Sha256Hex;
    internal static string _Sha256Hash;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var (md5, sha256, hostKey) = Helpers.GetServerFingerPrintsAndHostKey();
        _MD5 = md5;
        _Sha256Hex = Helpers.ConvertToSHA256Hex(hostKey);
        _Sha256Hash = sha256;
    }

    [Test]
    public async Task DeleteFiles_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _Sha256Hex;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public async Task DeleteFiles_TestTransferWithExpectedServerFingerprintAsHexSha256WithAltercations()
    {
        var connection = Helpers.GetSftpConnection();
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;
        connection.ServerFingerPrint = _Sha256Hash.Replace("=", string.Empty);
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public async Task DeleteFiles_TestTransferWithExpectedServerFingerprintAsSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _Sha256Hash;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public async Task DeleteFiles_TestTransferWithExpectedServerFingerprintAsMD5()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _MD5;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public async Task DeleteFiles_TestTransferWithExpectedServerFingerprintAsMD5ToLower()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _MD5.ToLower();
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public async Task DeleteFiles_TestTransferWithExpectedServerFingerprintAsMD5Hash()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _MD5.Replace(":", string.Empty);
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public void DeleteFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.DeleteFiles(_input, connection, default));
        Assert.That(ex.Message.StartsWith("Error when checking the server fingerprint"), ex.Message);
    }

    [Test]
    public void DeleteFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.DeleteFiles(_input, connection, default));
        Assert.That(ex.Message.StartsWith("Error when checking the server fingerprint"), ex.Message);
    }

    [Test]
    public void DeleteFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.DeleteFiles(_input, connection, default));
        Assert.That(ex.Message.StartsWith("Error when checking the server fingerprint"), ex.Message);
    }

    [Test]
    public void DeleteFiles_TestThrowsTransferWithInvalidExpectedServerFingerprint()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684x7RySiCwjGXmX2CfBaBHeSqO8vfiurenvire56";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.DeleteFiles(_input, connection, default));
        Assert.That(ex.Message.StartsWith("Error when checking the server fingerprint"), ex.Message);
    }

    [Test]
    public void DeleteFiles_TestShouldThrowWithoutPromptAndResponse()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePrivateKeyFile;
        connection.PrivateKeyFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");
        connection.Password = null;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.UseKeyboardInteractiveAuthentication = true;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;
        connection.ServerFingerPrint = _Sha256Hash.Replace("=", string.Empty);

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.DeleteFiles(_input, connection, default));
        Assert.AreEqual(
            "Failure in Keyboard-interactive authentication: No response given for server prompt request --> Password",
            ex.Message);

        connection.Authentication = AuthenticationType.UsernamePrivateKeyString;
        connection.PrivateKeyFile = null;
        connection.PrivateKeyString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "../../../Volumes/ssh_host_rsa_key"));
        connection.PrivateKeyPassphrase = "passphrase";

        _input.Directory = "upload/subDir";

        ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.DeleteFiles(_input, connection, default));
        Assert.AreEqual(
            "Failure in Keyboard-interactive authentication: No response given for server prompt request --> Password",
            ex.Message);
    }
}
