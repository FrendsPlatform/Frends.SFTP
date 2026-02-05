using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Frends.SFTP.ReadFile.Enums;
using Frends.SFTP.ReadFile.Tests.Lib;

namespace Frends.SFTP.ReadFile.Tests;

[TestFixture]
public class ServerFingerprintTests : ReadFileTestBase
{
    private static string md5;
    private static string sha256Hex;
    private static string sha256Hash;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var (newMd5, sha256, hostKey) = Helpers.GetServerFingerPrintsAndHostKey();
        md5 = newMd5;
        sha256Hex = Helpers.ConvertToSHA256Hex(hostKey);
        sha256Hash = sha256;
    }

    [Test]
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        Connection.ServerFingerPrint = sha256Hex;
        Connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);
    }

    [Test]
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsHexSha256WithAltercations()
    {
        Connection.ServerFingerPrint = sha256Hash.Replace("=", "");
        Connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);
    }

    [Test]
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsSha256()
    {
        Connection.ServerFingerPrint = sha256Hash;
        Connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);
    }

    [Test]
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsMD5()
    {
        Connection.ServerFingerPrint = md5;
        Connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);
    }

    [Test]
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsMD5ToLower()
    {
        Connection.ServerFingerPrint = md5.ToLower();
        Connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);
    }

    [Test]
    public async Task ReadFile_TestTransferWithExpectedServerFingerprintAsMD5Hash()
    {
        Connection.ServerFingerPrint = md5.Replace(":", "");
        Connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);
    }

    [Test]
    public void ReadFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        Connection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }

    [Test]
    public void ReadFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        Connection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }

    [Test]
    public void ReadFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        Connection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }

    [Test]
    public void ReadFile_TestThrowsTransferWithInvalidExpectedServerFingerprint()
    {
        Connection.ServerFingerPrint = "InvalidFingerprint";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }
}
