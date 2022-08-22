using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using System.Net.Sockets;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests;

[TestFixture]
public class ServerFingerprintTests : DownloadFilesTestBase
{
    internal static string _MD5;
    internal static string _Sha256Hex;
    internal static string _Sha256Hash;
    internal static string _Sha1;

    [OneTimeSetUp]
    public static void OneTimeSetup()
    {
        var (fingerPrint, hostKey) = Helpers.GetServerFingerPrintAndHostKey();
        _MD5 = Helpers.ConvertToMD5Hex(fingerPrint);
        _Sha1 = Helpers.ConvertToSHA1(hostKey);
        _Sha256Hex = Helpers.ConvertToSHA256Hex(hostKey);
        _Sha256Hash = Helpers.ConvertToSHA256Hash(hostKey);
    }

    [Test]
    public void DownloadFiles_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _Sha256Hex;

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.ActionSkipped);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestTransferWithExpectedServerFingerprintAsSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _Sha256Hash;

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.ActionSkipped);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestTransferWithExpectedServerFingerprintAsSha1()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _Sha1;

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.ActionSkipped);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestTransferWithExpectedServerFingerprintAsMD5()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _MD5;

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.ActionSkipped);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestTransferWithExpectedServerFingerprintAsMD5Hash()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = _MD5.Replace(":", "");

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsFalse(result.ActionSkipped);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsMD5()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "73:58:DF:2D:CD:12:35:AB:7D:00:41:F0:1E:62:15:E0";

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Error when establishing connection to the Server: Key exchange negotiation failed.."));
    }

    [Test]
    public void DownloadFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "c4b56fba6167c11f62e26b192c839d394e5c8d278b614b81345d037d178442f2";

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Error when establishing connection to the Server: Key exchange negotiation failed.."));
    }

    [Test]
    public void DownloadFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "FBQn5eyoxpAl33Ly0gyScCGAqZeMVsfY7qss3KOM/hY=";

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Error when establishing connection to the Server: Key exchange negotiation failed.."));
    }

    [Test]
    public void DownloadFiles_TestThrowsTransferWithInvalidExpectedServerFingerprintAsSha1()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = "f6b1fa0ac7d00c615c340c2f3c8db92d2fabe905";

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.IsTrue(ex.Message.StartsWith("SFTP transfer failed: Error when establishing connection to the Server: Key exchange negotiation failed.."));
    }
}

