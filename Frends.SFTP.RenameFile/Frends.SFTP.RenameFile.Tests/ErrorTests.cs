using NUnit.Framework;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Net.Sockets;
using Frends.SFTP.RenameFile.Definitions;
using Frends.SFTP.RenameFile.Enums;

namespace Frends.SFTP.RenameFile.Tests;

[TestFixture]
public class ErrorTests
{
    [Test]
    public void RenameFile_TestFileNotExistsThrows()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Path = "/upload/test.txt",
            NewFileName = "newTest.txt",
            RenameBehaviour = RenameBehaviour.Throw
        };

        var ex = Assert.ThrowsAsync<SftpPathNotFoundException>(async () => await SFTP.RenameFile(input, connection, default));
        Assert.That(ex.Message.Contains("No such file"), ex.Message);

    }

    [Test]
    public void RenameFile_TestThrowsWithWrongPort()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Port = 51644;
        var input = new Input
        {
            Path = "/upload/ t",
            RenameBehaviour = RenameBehaviour.Throw
        };

        Assert.ThrowsAsync<SocketException>(async () => await SFTP.RenameFile(input, connection, default));
    }

    [Test]
    public void RenameFile_TestThrowsWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Password = "demo";
        connection.Username = "demo";

        var input = new Input
        {
            Path = "/upload/test.txt",
            NewFileName = "newTest.txt",
            RenameBehaviour = RenameBehaviour.Throw
        };

        var ex = Assert.ThrowsAsync<SshAuthenticationException>(async () => await SFTP.RenameFile(input, connection, default));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public void RenameFile_TestThrowsWithIncorrectPrivateKeyPassphrase()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "demo";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Path = "/upload/test.txt",
            NewFileName = "newTest.txt",
            RenameBehaviour = RenameBehaviour.Throw
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.RenameFile(input, connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info:"));
    }

    [Test]
    public void RenameFile_TestThrowsWithEmptyPrivateKeyFile()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyFile = "";

        var input = new Input
        {
            Path = "/upload/test.txt",
            NewFileName = "newTest.txt",
            RenameBehaviour = RenameBehaviour.Throw
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.RenameFile(input, connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void RenameFile_TestThrowsWithIncorrectPrivateKeyString()
    {
        var key = Helpers.GenerateDummySshKey();

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyString = key.ToString();

        var input = new Input
        {
            Path = "/upload/test.txt",
            NewFileName = "newTest.txt",
            RenameBehaviour = RenameBehaviour.Throw
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.RenameFile(input, connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void RenameFile_TestThrowsWithIncorrectServerFingerprint()
    {
        var fingerprint = "f6:fc:1c:03:17:5f:67:4f:1f:0b:50:5a:9f:f9:30:e5";

        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = fingerprint;

        var input = new Input
        {
            Path = "/upload/test.txt",
            NewFileName = "newTest.txt",
            RenameBehaviour = RenameBehaviour.Throw
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.RenameFile(input, connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }
}

