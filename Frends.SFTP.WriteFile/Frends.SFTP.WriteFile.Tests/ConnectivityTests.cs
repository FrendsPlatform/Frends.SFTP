using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Frends.SFTP.WriteFile.Definitions;
using Frends.SFTP.WriteFile.Enums;

namespace Frends.SFTP.WriteFile.Tests;

[TestFixture]
public class ConnectivityTests : WriteFileTestBase
{
    [Test]
    public void WriteFile_TestWithLargerBuffer()
    {
        var connection = Helpers.GetSftpConnection();
        connection.BufferSize = 256;
        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var result = SFTP.WriteFile(input, connection, new CancellationToken());
        Assert.AreEqual("/write/test.txt", result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));
        Assert.AreEqual("test", Helpers.GetDestinationFileContent(input.Path));
    }

    [Test]
    public void WriteFile_TestWithMD5ServerFingerprint()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Helpers.GetServerFingerprintAsMD5String();
        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var result = SFTP.WriteFile(input, connection, new CancellationToken());
        Assert.AreEqual("/write/test.txt", result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));
        Assert.AreEqual("test", Helpers.GetDestinationFileContent(input.Path));
    }

    [Test]
    public void WriteFile_TestWithSHA256ServerFingerprint()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Helpers.GetServerFingerprintAsSHA256String();
        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var result = SFTP.WriteFile(input, connection, new CancellationToken());
        Assert.AreEqual("/write/test.txt", result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));
        Assert.AreEqual("test", Helpers.GetDestinationFileContent(input.Path));
    }

    [Test]
    public void WriteFile_TestWithPrivateKeyFileRsa()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var result = SFTP.WriteFile(input, connection, new CancellationToken());
        Assert.AreEqual("/write/test.txt", result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));
        Assert.AreEqual("test", Helpers.GetDestinationFileContent(input.Path));
    }

    [Test]
    public void WriteFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyString = key;

        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var result = SFTP.WriteFile(input, connection, new CancellationToken());
        Assert.AreEqual("/write/test.txt", result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));
        Assert.AreEqual("test", Helpers.GetDestinationFileContent(input.Path));
    }
}

