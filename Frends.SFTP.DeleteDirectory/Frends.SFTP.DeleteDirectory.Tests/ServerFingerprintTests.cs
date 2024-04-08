namespace Frends.SFTP.DeleteDirectory.Tests;

using System;
using System.IO;
using System.Threading.Tasks;
using Frends.SFTP.DeleteDirectory.Definitions;
using Frends.SFTP.DeleteDirectory.Enums;
using NUnit.Framework;
using Renci.SshNet.Common;

[TestFixture]
public class ServerFingerprintTests : UnitTestBase
{
    internal string _MD5;
    internal string _Sha256Hex;
    internal string _Sha256Hash;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var (md5, sha256, hostKey) = Helpers.GetServerFingerPrintsAndHostKey();
        _MD5 = md5;
        _Sha256Hex = Helpers.ConvertToSHA256Hex(hostKey);
        _Sha256Hash = sha256;
    }

    [Test]
    public async Task DeleteDirectory_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _Sha256Hex;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var options = _options;
        options.ThrowNotExistError = NotExistsOptions.Skip;

        var result = await SFTP.DeleteDirectory(_input, connection, _options, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.ErrorMessage);
        Assert.NotNull(result.Data);
    }

    [Test]
    public async Task DeleteDirectory_TestTransferWithExpectedServerFingerprintAsHexSha256WithAltercations()
    {
        var connection = Helpers.GetSftpConnection();
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;
        connection.ServerFingerPrint = _Sha256Hash.Replace("=", string.Empty);
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.DeleteDirectory(_input, connection, _options, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.ErrorMessage);
    }

    [Test]
    public async Task DeleteDirectory_TestTransferWithExpectedServerFingerprintAsSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _Sha256Hash;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.DeleteDirectory(_input, connection, _options, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.ErrorMessage);
    }

    [Test]
    public async Task DeleteDirectory_TestTransferWithExpectedServerFingerprintAsMD5()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _MD5;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.DeleteDirectory(_input, connection, _options, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.ErrorMessage);
    }

    [Test]
    public async Task DeleteDirectory_TestTransferWithExpectedServerFingerprintAsMD5ToLower()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _MD5.ToLower();
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.DeleteDirectory(_input, connection, _options, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.ErrorMessage);
    }

    [Test]
    public async Task DeleteDirectory_TestTransferWithExpectedServerFingerprintAsMD5Hash()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _MD5.Replace(":", string.Empty);
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.DeleteDirectory(_input, connection, _options, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.ErrorMessage);
    }

    [Test]
    public void DeleteDirectory_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.DeleteDirectory(_input, connection, _options, default));
        Assert.AreEqual("Key exchange negotiation failed.", ex.Message);
    }

    [Test]
    public async Task DeleteDirectory_TestReturnsErrorTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var ex = await SFTP.DeleteDirectory(_input, connection, options, default);
        Assert.IsFalse(ex.Success);
        Assert.AreEqual("Key exchange negotiation failed.", ex.ErrorMessage.Message);
    }

    [Test]
    public void DeleteDirectory_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.DeleteDirectory(_input, connection, _options, default));
        Assert.AreEqual("Key exchange negotiation failed.", ex.Message);
    }

    [Test]
    public async Task DeleteDirectory_TestReturnsErrorTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var ex = await SFTP.DeleteDirectory(_input, connection, options, default);
        Assert.IsFalse(ex.Success);
        Assert.AreEqual("Key exchange negotiation failed.", ex.ErrorMessage.Message);
    }

    [Test]
    public void DeleteDirectory_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.DeleteDirectory(_input, connection, _options, default));
        Assert.AreEqual("Key exchange negotiation failed.", ex.Message);
    }

    [Test]
    public async Task DeleteDirectory_TestReturnsErrorTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var ex = await SFTP.DeleteDirectory(_input, connection, options, default);
        Assert.IsFalse(ex.Success);
        Assert.AreEqual("Key exchange negotiation failed.", ex.ErrorMessage.Message);
    }

    [Test]
    public void DeleteDirectory_TestThrowsTransferWithInvalidExpectedServerFingerprint()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684x7RySiCwjGXmX2CfBaBHeSqO8vfiurenvire56";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.DeleteDirectory(_input, connection, _options, default));
        Assert.AreEqual("Key exchange negotiation failed.", ex.Message);
    }

    [Test]
    public async Task DeleteDirectory_TestReturnsErrorTransferWithInvalidExpectedServerFingerprint()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684x7RySiCwjGXmX2CfBaBHeSqO8vfiurenvire56";
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var ex = await SFTP.DeleteDirectory(_input, connection, options, default);
        Assert.IsFalse(ex.Success);
        Assert.AreEqual("Key exchange negotiation failed.", ex.ErrorMessage.Message);
    }

    [Test]
    public void DeleteDirectory_TestShouldThrowWithoutPromptAndResponse()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePrivateKeyFile;
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");
        connection.Password = null;
        connection.PrivateKeyPassphrase = Guid.NewGuid().ToString();
        connection.UseKeyboardInteractiveAuthentication = true;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;
        connection.ServerFingerPrint = _Sha256Hash.Replace("=", string.Empty);

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.DeleteDirectory(_input, connection, _options, default));
        Assert.AreEqual("Failure in Keyboard-interactive authentication: No response given for server prompt request --> Password", ex.Message);

        connection.Authentication = AuthenticationType.UsernamePrivateKeyString;
        connection.PrivateKeyFile = null;
        connection.PrivateKeyString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));
        connection.PrivateKeyPassphrase = Guid.NewGuid().ToString();

        _input.Directory = "upload/subDir";

        ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.DeleteDirectory(_input, connection, _options, default));
        Assert.AreEqual("Failure in Keyboard-interactive authentication: No response given for server prompt request --> Password", ex.Message);
    }

    [Test]
    public async Task DeleteDirectory_TestShouldReturnErrorWithoutPromptAndResponse()
    {
        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePrivateKeyFile;
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");
        connection.Password = null;
        connection.PrivateKeyPassphrase = Guid.NewGuid().ToString();
        connection.UseKeyboardInteractiveAuthentication = true;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;
        connection.ServerFingerPrint = _Sha256Hash.Replace("=", string.Empty);

        var ex = await SFTP.DeleteDirectory(_input, connection, options, default);
        Assert.IsFalse(ex.Success);
        Assert.IsTrue(ex.ErrorMessage.Message.Contains("Failure in Keyboard-interactive authentication: No response given for server prompt request --> Password"));

        connection.Authentication = AuthenticationType.UsernamePrivateKeyString;
        connection.PrivateKeyFile = null;
        connection.PrivateKeyString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));
        connection.PrivateKeyPassphrase = Guid.NewGuid().ToString();

        _input.Directory = "upload/subDir";

        var ex2 = await SFTP.DeleteDirectory(_input, connection, options, default);
        Assert.IsFalse(ex2.Success);
        Assert.IsTrue(ex2.ErrorMessage.Message.Contains("Failure in Keyboard-interactive authentication: No response given for server prompt request --> Password"));
    }
}