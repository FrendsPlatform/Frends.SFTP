using System;
using System.IO;
using NUnit.Framework;
using Frends.SFTP.ReadFile.Definitions;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile.Tests;

[TestFixture]
public class ConnectivityTests
{
    [Test]
    public void ReadFile_TestWithLargerBuffer()
    {
        var connection = Helpers.GetSftpConnection();
        connection.BufferSize = 256;
        var input = new Input
        {
            Path = "/read/test.txt",
            FileEncoding = FileEncoding.ANSI,
        };

        Helpers.GenerateDummyFile(input.Path, "test");

        var result = SFTP.ReadFile(input, connection);
        Assert.AreEqual("/read/test.txt", result.Path);
        Assert.AreEqual("test", result.Content);

        Helpers.DeleteSourceFile(input.Path);
    }

    [Test]
    public void ReadFile_TestWithMD5ServerFingerprint()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Helpers.GetServerFingerprintAsMD5String();
        var input = new Input
        {
            Path = "/read/test.txt",
            FileEncoding = FileEncoding.ANSI,
        };

        Helpers.GenerateDummyFile(input.Path, "test");

        var result = SFTP.ReadFile(input, connection);
        Assert.AreEqual("/read/test.txt", result.Path);
        Assert.AreEqual("test", result.Content);

        Helpers.DeleteSourceFile(input.Path);
    }

    [Test]
    public void ReadFile_TestWithSHA256ServerFingerprint()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Helpers.GetServerFingerprintAsSHA256String();
        var input = new Input
        {
            Path = "/read/test.txt",
            FileEncoding = FileEncoding.ANSI,
        };

        Helpers.GenerateDummyFile(input.Path, "test");

        var result = SFTP.ReadFile(input, connection);
        Assert.AreEqual("/read/test.txt", result.Path);
        Assert.AreEqual("test", result.Content);

        Helpers.DeleteSourceFile(input.Path);
    }

    [Test]
    public void ReadFile_TestWithPrivateKeyFileRsa()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Path = "/read/test.txt",
            FileEncoding = FileEncoding.ANSI,
        };

        Helpers.GenerateDummyFile(input.Path, "test");

        var result = SFTP.ReadFile(input, connection);
        Assert.AreEqual("/read/test.txt", result.Path);
        Assert.AreEqual("test", result.Content);

        Helpers.DeleteSourceFile(input.Path);
    }

    [Test]
    public void ReadFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyString = key;

        var input = new Input
        {
            Path = "/read/test.txt",
            FileEncoding = FileEncoding.ANSI,
        };

        Helpers.GenerateDummyFile(input.Path, "test");

        var result = SFTP.ReadFile(input, connection);
        Assert.AreEqual("/read/test.txt", result.Path);
        Assert.AreEqual("test", result.Content);

        Helpers.DeleteSourceFile(input.Path);
    }
}

