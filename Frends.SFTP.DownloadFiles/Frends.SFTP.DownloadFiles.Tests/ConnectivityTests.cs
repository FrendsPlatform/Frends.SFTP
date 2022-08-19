using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests;

[TestFixture]
public class ConnectivityTests : DownloadFilesTestBase
{
    [Test]
    public void DownloadFiles_TestWithLargerBuffer()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, "LargeTestFile.bin") }, _source.Directory);

        var connection = Helpers.GetSftpConnection();
        connection.BufferSize = 256;

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "LargeTestFile.bin",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        var result = SFTP.DownloadFiles(source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestTransferThatThrowsWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ConnectionTimeout = 10;
        connection.UserName = "demo";
        connection.Password = "demo";

        var result = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.That(result.Message.StartsWith("SFTP transfer failed: Authentication of SSH session failed: Permission denied (password)"));
    }

    [Test]
    public void DownloadFiles_TestPrivateKeyFileRsa()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyString = key;

        var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestTransferWithExpectedServerFingerprintAsHexSha256()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Helpers.GetServerFingerprintAsSHA256HexString();

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
        connection.ServerFingerPrint = Helpers.GetServerFingerprintAsSHA256Base64String();

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
        connection.ServerFingerPrint = Helpers.GetServerFingerprintAsSHA1String();

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
        connection.ServerFingerPrint = Helpers.GetServerFingerprintAsMD5HexString();

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
        connection.ServerFingerPrint = Helpers.GetServerFingerprintAsMD5HexString().Replace(":", "");

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
}

