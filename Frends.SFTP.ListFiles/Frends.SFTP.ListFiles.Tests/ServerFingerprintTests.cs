using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.ListFiles.Enums;
using Frends.SFTP.ListFiles.Tests.Lib;
using NUnit.Framework;
using Renci.SshNet.Common;

namespace Frends.SFTP.ListFiles.Tests;

[TestFixture]
public class ServerFingerprintTests : ListFilesTestBase
{
    private static string Md5 { get; set; }

    private static string Sha256Hex { get; set; }

    private static string Sha256Hash { get; set; }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var (mD5, sHA256, hostKey) = Lib.Helpers.GetServerFingerPrintsAndHostKey();
        Md5 = mD5;
        Sha256Hex = Lib.Helpers.ConvertToSHA256Hex(hostKey);
        Sha256Hash = sHA256;
    }

    [Test]
    public async Task ListFiles_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Sha256Hex;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ListFiles(Input, connection, Options, CancellationToken.None);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public async Task ListFiles_TestTransferWithExpectedServerFingerprintAsHexSha256WithAltercations()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Sha256Hash.Replace("=", string.Empty);
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ListFiles(Input, connection, Options, CancellationToken.None);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public async Task ListFiles_TestTransferWithExpectedServerFingerprintAsSha256()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Sha256Hash;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ListFiles(Input, connection, Options, CancellationToken.None);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public async Task ListFiles_TestTransferWithExpectedServerFingerprintAsMD5()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Md5;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ListFiles(Input, connection, Options, CancellationToken.None);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public async Task ListFiles_TestTransferWithExpectedServerFingerprintAsMD5ToLower()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Md5.ToLower();
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ListFiles(Input, connection, Options, CancellationToken.None);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public async Task ListFiles_TestTransferWithExpectedServerFingerprintAsMD5Hash()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Md5.Replace(":", string.Empty);
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.ListFiles(Input, connection, Options, CancellationToken.None);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public void ListFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.ListFiles(Input, connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"));
    }

    [Test]
    public void ListFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.ListFiles(Input, connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"));
    }

    [Test]
    public void ListFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.ListFiles(Input, connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"));
    }

    [Test]
    public void ListFiles_TestThrowsTransferWithInvalidExpectedServerFingerprint()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684x7RySiCwjGXmX2CfBaBHeSqO8vfiurenvire56";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.ListFiles(Input, connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"));
    }
}
