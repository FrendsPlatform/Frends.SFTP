using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Frends.SFTP.MoveFile.Definitions;
using Frends.SFTP.MoveFile.Enums;
using NUnit.Framework;
using Renci.SshNet.Common;

namespace Frends.SFTP.MoveFile.Tests;

[TestFixture]
internal class ErrorTests
{
    [Test]
    public async Task MoveFile_TestNoSourceFilesFound()
    {
        var connection = Helpers.GetSftpConnection();

        var input = new Input
        {
            Directory = "/upload",
            Pattern = "filenotexisting.txt",
            TargetDirectory = "/upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw,
        };

        var result = await SFTP.MoveFile(input, connection, new Options(), default);

        Assert.IsNotNull(result);
        Assert.AreEqual("No files were found matching the given pattern.", result.Message);
        Assert.AreEqual(0, result.Files.Count);
    }

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
            IfTargetFileExists = FileExistsOperation.Throw,
        };

        Assert.ThrowsAsync<SocketException>(async () => await SFTP.MoveFile(input, connection, new Options(), default));
    }

    [Test]
    public async Task MoveFile_ReturnsErrorWhenConfiguredNotToThrow()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Port = 51644;
        var input = new Input
        {
            Directory = "/upload",
            Pattern = "filenotexisting.txt",
            TargetDirectory = "/upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw,
        };
        var options = new Options { ThrowErrorOnFailure = false };

        var result = await SFTP.MoveFile(input, connection, options, default);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNotNull(result.Error.AdditionalInfo);
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
            IfTargetFileExists = FileExistsOperation.Throw,
        };

        var ex = Assert.ThrowsAsync<SshAuthenticationException>(async () => await SFTP.MoveFile(input, connection, new Options(), default));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public void MoveFile_TestThrowsWithIncorrectPrivateKeyPassphrase()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "demo";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Directory = "/upload",
            Pattern = "filenotexisting.txt",
            TargetDirectory = "/upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw,
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.MoveFile(input, connection, new Options(), default));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info:"));
    }

    [Test]
    public void MoveFile_TestThrowsWithEmptyPrivateKeyFile()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyFile = string.Empty;

        var input = new Input
        {
            Directory = "/upload",
            Pattern = "filenotexisting.txt",
            TargetDirectory = "/upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw,
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.MoveFile(input, connection, new Options(), default));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void MoveFile_TestThrowsWithIncorrectPrivateKeyString()
    {
        var key = Helpers.GenerateDummySshKey();

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyString = key.ToString();

        var input = new Input
        {
            Directory = "/upload",
            Pattern = "filenotexisting.txt",
            TargetDirectory = "/upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw,
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.MoveFile(input, connection, new Options(), default));
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
            IfTargetFileExists = FileExistsOperation.Throw,
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.MoveFile(input, connection, new Options(), default));
        Assert.That(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }
}
