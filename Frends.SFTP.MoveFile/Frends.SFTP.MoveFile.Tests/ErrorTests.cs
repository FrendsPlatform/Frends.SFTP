using NUnit.Framework;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Net.Sockets;
using Frends.SFTP.MoveFile.Definitions;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile.Tests;

[TestFixture]
public class ErrorTests
{
    [Test]
    public void MoveFile_TestThrowsWithWrongPort()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Port = 51644;
        var input = new Input
        {
            Directory = "/upload",
            Pattern = "filenotexisting.txt",
            TargetDirectory = "/upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw
        };

        Assert.Throws<SocketException>(() => SFTP.MoveFile(input, connection, default));
    }

    [Test]
    public void MoveFile_TestThrowsWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Password = "demo";
        connection.Username = "demo";

        var input = new Input
        {
            Directory = "/upload",
            Pattern = "filenotexisting.txt",
            TargetDirectory = "/upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw
        };

        var ex = Assert.Throws<SshAuthenticationException>(() => SFTP.MoveFile(input, connection, default));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public void MoveFile_TestThrowsWithIncorrectPrivateKeyPassphrase()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "demo";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Directory = "/upload",
            Pattern = "filenotexisting.txt",
            TargetDirectory = "/upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.MoveFile(input, connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info:"));
    }

    [Test]
    public void MoveFile_TestThrowsWithEmptyPrivateKeyFile()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyFile = "";

        var input = new Input
        {
            Directory = "/upload",
            Pattern = "filenotexisting.txt",
            TargetDirectory = "/upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.MoveFile(input, connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void MoveFile_TestThrowsWithIncorrectPrivateKeyString()
    {
        var key = Helpers.GenerateDummySshKey();

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyString = key.ToString();

        var input = new Input
        {
            Directory = "/upload",
            Pattern = "filenotexisting.txt",
            TargetDirectory = "/upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.MoveFile(input, connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void MoveFile_TestThrowsWithIncorrectServerFingerprint()
    {
        var fingerprint = "f6:fc:1c:03:17:5f:67:4f:1f:0b:50:5a:9f:f9:30:e5";

        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = fingerprint;

        var input = new Input
        {
            Directory = "/upload",
            Pattern = "filenotexisting.txt",
            TargetDirectory = "/upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw
        };

        var ex = Assert.Throws<SshConnectionException>(() => SFTP.MoveFile(input, connection, default));
        Assert.AreEqual("Key exchange negotiation failed.", ex.Message);
    }
}

