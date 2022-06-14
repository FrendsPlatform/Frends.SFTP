using NUnit.Framework;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using Frends.SFTP.WriteFile.Definitions;
using Frends.SFTP.WriteFile.Enums;

namespace Frends.SFTP.WriteFile.Tests;

[TestFixture]
public class ErrorTests : WriteFileTestBase
{
    [Test]
    public void WriteFile_TestFileExistsThrows()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var result = SFTP.WriteFile(input, connection, new CancellationToken());
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(input, connection, new CancellationToken()));
        Assert.AreEqual($"File already exists: {input.Path}", ex.Message);

        Helpers.DeleteDestinationFile(input.Path);
    }

    [Test]
    public void WriteFile_TestThrowsWithWrongPort()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Port = 51644;
        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        Assert.Throws<SocketException>(() => SFTP.WriteFile(input, connection, new CancellationToken()));
    }

    [Test]
    public void WriteFile_TestThrowsWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Password = "demo";
        connection.UserName = "demo";

        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var ex = Assert.Throws<SshAuthenticationException>(() => SFTP.WriteFile(input, connection, new CancellationToken()));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public void WriteFile_TestThrowsWithIncorrectPrivateKeyPassphrase()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "demo";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(input, connection, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("Error when initializing connection info:"));
    }

    [Test]
    public void WriteFile_TestThrowsWithEmptyPrivateKeyFile()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyFile = "";

        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(input, connection, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void WriteFile_TestThrowsWithIncorrectPrivateKeyString()
    {
        var key = Helpers.GenerateDummySshKey();

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyString = key.ToString();

        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(input, connection, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void WriteFile_TestThrowsWithIncorrectServerFingerprint()
    {
        var fingerprint = "f6:fc:1c:03:17:5f:67:4f:1f:0b:50:5a:9f:f9:30:e5";

        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = fingerprint;

        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var ex = Assert.Throws<SshConnectionException>(() => SFTP.WriteFile(input, connection, new CancellationToken()));
        Assert.AreEqual("Key exchange negotiation failed.", ex.Message);
    }
}

