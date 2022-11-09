﻿using System;
using System.Threading;
using NUnit.Framework;
using Renci.SshNet.Common;
using Frends.SFTP.ListFiles.Definitions;

namespace Frends.SFTP.ListFiles.Tests;

[TestFixture]
public class ServerFingerprintTests : ListFilesTestBase
{
    internal static string _MD5;
    internal static string _Sha256Hex;
    internal static string _Sha256Hash;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var (fingerPrint, hostKey) = Helpers.GetServerFingerPrintAndHostKey();
        _MD5 = Helpers.ConvertToMD5Hex(fingerPrint);
        _Sha256Hex = Helpers.ConvertToSHA256Hex(hostKey);
        _Sha256Hash = Helpers.ConvertToSHA256Hash(hostKey);
    }

    [Test]
    public void ListFiles_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _Sha256Hex;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = SFTP.ListFiles(_input, connection, new CancellationToken());
        Assert.AreEqual(3, result.Count);
    }

    [Test]
    public void ListFiles_TestTransferWithExpectedServerFingerprintAsHexSha256WithAltercations()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _Sha256Hash.Replace("=", "");
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = SFTP.ListFiles(_input, connection, new CancellationToken());
        Assert.AreEqual(3, result.Count);
    }

    [Test]
    public void ListFiles_TestTransferWithExpectedServerFingerprintAsSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _Sha256Hash;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = SFTP.ListFiles(_input, connection, new CancellationToken());
        Assert.AreEqual(3, result.Count);
    }

    [Test]
    public void ListFiles_TestTransferWithExpectedServerFingerprintAsMD5()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _MD5;
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = SFTP.ListFiles(_input, connection, new CancellationToken());
        Assert.AreEqual(3, result.Count);
    }

    [Test]
    public void ListFiles_TestTransferWithExpectedServerFingerprintAsMD5ToLower()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _MD5.ToLower();
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = SFTP.ListFiles(_input, connection, new CancellationToken());
        Assert.AreEqual(3, result.Count);
    }

    [Test]
    public void ListFiles_TestTransferWithExpectedServerFingerprintAsMD5Hash()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _MD5.Replace(":", "");
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;

        var result = SFTP.ListFiles(_input, connection, new CancellationToken());
        Assert.AreEqual(3, result.Count);
    }

    [Test]
    public void ListFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";

        var ex = Assert.Throws<SshConnectionException>(() => SFTP.ListFiles(_input, connection, new CancellationToken()));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
    }

    [Test]
    public void ListFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";

        var ex = Assert.Throws<SshConnectionException>(() => SFTP.ListFiles(_input, connection, new CancellationToken()));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
    }

    [Test]
    public void ListFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684+x+7RySiCwj+GXmX2CfBaBHeSqO8=";

        var ex = Assert.Throws<SshConnectionException>(() => SFTP.ListFiles(_input, connection, new CancellationToken()));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
    }

    [Test]
    public void ListFiles_TestThrowsTransferWithInvalidExpectedServerFingerprint()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "nuDEsWN4tfEQ684x7RySiCwjGXmX2CfBaBHeSqO8vfiurenvire56";

        var ex = Assert.Throws<SshConnectionException>(() => SFTP.ListFiles(_input, connection, new CancellationToken()));
        Assert.IsTrue(ex.Message.StartsWith("Key exchange negotiation failed."));
        //Assert.That(ex.Message.Contains("Expected server fingerprint was given in unsupported format."));
    }
}
