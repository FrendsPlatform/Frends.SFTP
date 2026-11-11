using System;
using System.Threading.Tasks;
using Frends.SFTP.RenameFile.Enums;
using NUnit.Framework;
using Renci.SshNet.Common;

namespace Frends.SFTP.RenameFile.Tests;

[TestFixture]
public class ServerFingerprintTests : RenameFileTestBase
{
    private static string md5Fingerprint;
    private static string sha256Hex;
    private static string sha256Hash;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var (md5, sha256, hostKey) = Helpers.GetServerFingerPrintsAndHostKey();
        md5Fingerprint = md5;
        sha256Hex = Helpers.ConvertToSHA256Hex(hostKey);
        sha256Hash = sha256;
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        TestConnection.ServerFingerPrint = sha256Hex;
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsHexSha256WithAltercations()
    {
        TestConnection.ServerFingerPrint = sha256Hash.Replace("=", string.Empty);
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsSha256()
    {
        TestConnection.ServerFingerPrint = sha256Hash;
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsMD5()
    {
        TestConnection.ServerFingerPrint = md5Fingerprint;
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsMD5ToLower()
    {
        TestConnection.ServerFingerPrint = md5Fingerprint.ToLower();
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestTransferWithExpectedServerFingerprintAsMD5Hash()
    {
        TestConnection.ServerFingerPrint = md5Fingerprint.Replace(":", string.Empty);
        TestConnection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public void RenameFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        TestConnection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }

    [Test]
    public void RenameFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        TestConnection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }

    [Test]
    public void RenameFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        TestConnection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }

    [Test]
    public void RenameFile_TestThrowsTransferWithInvalidExpectedServerFingerprint()
    {
        TestConnection.ServerFingerPrint = "nuDEsWN4tfEQ684x7RySiCwjGXmX2CfBaBHeSqO8vfiurenvire56";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }
}
