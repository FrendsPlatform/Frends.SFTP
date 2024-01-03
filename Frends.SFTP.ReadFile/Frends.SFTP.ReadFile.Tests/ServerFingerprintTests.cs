using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Renci.SshNet.Common;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile.Tests;

[TestFixture]
public class ServerFingerprintTests : ReadFileTestBase
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
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        _connection.ServerFingerPrint = _Sha256Hex;
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsHexSha256WithAltercations()
    {
        _connection.ServerFingerPrint = _Sha256Hash.Replace("=", "");
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsSha256()
    {
        _connection.ServerFingerPrint = _Sha256Hash;
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsMD5()
    {
        _connection.ServerFingerPrint = _MD5;
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsMD5ToLower()
    {
        _connection.ServerFingerPrint = _MD5.ToLower();
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsMD5Hash()
    {
        _connection.ServerFingerPrint = _MD5.Replace(":", "");
        _connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public void ReadFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        _connection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.ReadFile(_input, _connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
    }

    [Test]
    public void ReadFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        _connection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.ReadFile(_input, _connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
    }

    [Test]
    public void ReadFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        _connection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.ReadFile(_input, _connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
    }

    [Test]
    public void ReadFile_TestThrowsTransferWithInvalidExpectedServerFingerprint()
    {
        _connection.ServerFingerPrint = "nuDEsWN4tfEQ684x7RySiCwjGXmX2CfBaBHeSqO8vfiurenvire56";

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.ReadFile(_input, _connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
    }
}