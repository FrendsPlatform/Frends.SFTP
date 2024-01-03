using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Renci.SshNet.Common;
using Frends.SFTP.RenameFile.Enums;

namespace Frends.SFTP.RenameFile.Tests;

[TestFixture]
public class ServerFingerprintTests : RenameFileTestBase
{
    private static string _MD5;
    private static string _Sha256Hex;
    private static string _Sha256Hash;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var (MD5, SHA256, hostKey) = Helpers.GetServerFingerPrintsAndHostKey();
        _MD5 = MD5;
        _Sha256Hex = Helpers.ConvertToSHA256Hex(hostKey);
        _Sha256Hash = SHA256;
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        _connection.ServerFingerPrint = _Sha256Hex;
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsHexSha256WithAltercations()
    {
        _connection.ServerFingerPrint = _Sha256Hash.Replace("=", "");
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsSha256()
    {
        _connection.ServerFingerPrint = _Sha256Hash;
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsMD5()
    {
        _connection.ServerFingerPrint = _MD5;
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsMD5ToLower()
    {
        _connection.ServerFingerPrint = _MD5.ToLower();
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsMD5Hash()
    {
        _connection.ServerFingerPrint = _MD5.Replace(":", "");
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public void RenameFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        _connection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.RenameFile(_input, _connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
    }

    [Test]
    public void RenameFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        _connection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.RenameFile(_input, _connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
    }

    [Test]
    public void RenameFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        _connection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.RenameFile(_input, _connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
    }

    [Test]
    public void RenameFile_TestThrowsTransferWithInvalidExpectedServerFingerprint()
    {
        _connection.ServerFingerPrint = "nuDEsWN4tfEQ684x7RySiCwjGXmX2CfBaBHeSqO8vfiurenvire56";

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.RenameFile(_input, _connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
    }
}