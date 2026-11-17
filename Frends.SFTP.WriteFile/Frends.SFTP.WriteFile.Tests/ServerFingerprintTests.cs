using System;
using Frends.SFTP.WriteFile.Definitions;
using Frends.SFTP.WriteFile.Enums;
using NUnit.Framework;
using Renci.SshNet.Common;

namespace Frends.SFTP.WriteFile.Tests;

[TestFixture]
public class ServerFingerprintTests : WriteFileTestBase
{
    internal static string Md5 { get; set; }

    internal static string Sha256Hex { get; set; }

    internal static string Sha256Hash { get; set; }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var (md5, sha256, hostKey) = Helpers.GetServerFingerPrintsAndHostKey();
        Md5 = md5;
        Sha256Hex = Helpers.ConvertToSHA256Hex(hostKey);
        Sha256Hash = sha256;
    }

    [Test]
    public void WriteFile_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Sha256Hex;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        SFTP.WriteFile(TestInput, connection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestTransferWithExpectedServerFingerprintAsHexSha256WithAltercations()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Sha256Hash.Replace("=", string.Empty);
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        SFTP.WriteFile(TestInput, connection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestTransferWithExpectedServerFingerprintAsSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Sha256Hash;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        SFTP.WriteFile(TestInput, connection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestTransferWithExpectedServerFingerprintAsMD5()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Md5;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        SFTP.WriteFile(TestInput, connection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestTransferWithExpectedServerFingerprintAsMD5ToLower()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Md5.ToLower();
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        SFTP.WriteFile(TestInput, connection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestTransferWithExpectedServerFingerprintAsMD5Hash()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Md5.Replace(":", string.Empty);
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        SFTP.WriteFile(TestInput, connection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(TestInput, connection, TestOptions));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }

    [Test]
    public void WriteFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(TestInput, connection, TestOptions));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }

    [Test]
    public void WriteFile_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(TestInput, connection, TestOptions));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }

    [Test]
    public void WriteFile_TestThrowsTransferWithInvalidExpectedServerFingerprint()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684x7RySiCwjGXmX2CfBaBHeSqO8vfiurenvire56";

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(TestInput, connection, TestOptions));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }
}
