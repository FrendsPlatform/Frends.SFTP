using System;
using System.Threading.Tasks;
using Frends.SFTP.MoveFile.Enums;
using NUnit.Framework;
using Renci.SshNet.Common;

namespace Frends.SFTP.MoveFile.Tests;

[TestFixture]
internal class ServerFingerprintTests : MoveFileTestBase
{
    private static string mD5;
    private static string sha256Hex;
    private static string sha256Hash;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var (mD5, sHA256, hostKey) = Helpers.GetServerFingerPrintsAndHostKey();
        ServerFingerprintTests.mD5 = mD5;
        sha256Hex = Helpers.ConvertToSHA256Hex(hostKey);
        sha256Hash = sHA256;
    }

    [Test]
    public async Task MoveFile_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        TestConnection.ServerFingerPrint = sha256Hex;
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public async Task MoveFile_TestTransferWithExpectedServerFingerprintAsHexSha256WithAltercations()
    {
        TestConnection.ServerFingerPrint = sha256Hash.Replace("=", string.Empty);
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public async Task MoveFile_TestTransferWithExpectedServerFingerprintAsSha256()
    {
        TestConnection.ServerFingerPrint = sha256Hash;
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public async Task MoveFile_TestTransferWithExpectedServerFingerprintAsMD5()
    {
        TestConnection.ServerFingerPrint = mD5;
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public async Task MoveFile_TestTransferWithExpectedServerFingerprintAsMD5ToLower()
    {
        TestConnection.ServerFingerPrint = mD5.ToLower();
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public async Task MoveFile_TestTransferWithExpectedServerFingerprintAsMD5Hash()
    {
        TestConnection.ServerFingerPrint = mD5.Replace(":", string.Empty);
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public void MoveFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        TestConnection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"));
    }

    [Test]
    public void MoveFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        TestConnection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"));
    }

    [Test]
    public void MoveFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        TestConnection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"));
    }

    [Test]
    public void MoveFile_TestThrowsTransferWithInvalidExpectedServerFingerprint()
    {
        TestConnection.ServerFingerPrint = "nuDEsWN4tfEQ684x7RySiCwjGXmX2CfBaBHeSqO8vfiurenvire56";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"));
    }
}
