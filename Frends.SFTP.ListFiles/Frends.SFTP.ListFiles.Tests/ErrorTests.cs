using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.ListFiles.Definitions;
using Frends.SFTP.ListFiles.Enums;
using NUnit.Framework;
using Renci.SshNet.Common;

namespace Frends.SFTP.ListFiles.Tests;

[TestFixture]
public class ErrorTests : ListFilesTestBase
{
    [Test]
    public async Task ListFiles_TestDirectoryNotExistsCanReturnError()
    {
        var input = new Input
        {
            Directory = "/upload/nonexisting/directory",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI,
        };
        var options = new Options { ThrowErrorOnFailure = false };

        var result = await SFTP.ListFiles(input, Connection, options, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.That(result.Error.Message, Does.Contain("No such file"));
    }

    [Test]
    public void ListFiles_TestDirectoryNotExistsThrows()
    {
        var input = new Input
        {
            Directory = "/upload/nonexisting/directory",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI,
        };

        var ex = Assert.ThrowsAsync<SftpPathNotFoundException>(async () => await SFTP.ListFiles(input, Connection, Options, CancellationToken.None));
        Assert.That(ex.Message.StartsWith("No such file"), ex.Message);
    }

    [Test]
    public void ListFiles_TestThrowsWithWrongPort()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Port = 51644;
        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI,
        };

        Assert.ThrowsAsync<SocketException>(async () => await SFTP.ListFiles(input, connection, Options, CancellationToken.None));
    }

    [Test]
    public void ListFiles_TestThrowsWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Password = "demo";
        connection.Username = "demo";

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI,
        };

        var ex = Assert.ThrowsAsync<SshAuthenticationException>(async () => await SFTP.ListFiles(input, connection, Options, CancellationToken.None));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public void ListFiles_TestThrowsWithIncorrectPrivateKeyPassphrase()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "demo";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI,
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.ListFiles(input, connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info:"));
    }

    [Test]
    public void ListFiles_TestThrowsWithEmptyPrivateKeyFile()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyFile = string.Empty;

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI,
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.ListFiles(input, connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void ListFiles_TestThrowsWithIncorrectPrivateKeyString()
    {
        var key = Helpers.GenerateDummySshKey();

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyString = key.ToString();

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI,
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.ListFiles(input, connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void ListFiles_TestThrowsWithIncorrectServerFingerprint()
    {
        var fingerprint = "f6:fc:1c:03:17:5f:67:4f:1f:0b:50:5a:9f:f9:30:e5";

        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = fingerprint;

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI,
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.ListFiles(input, connection, Options, CancellationToken.None));
        Assert.That(ex.Message.Contains("Error when checking the server fingerprint:"), ex.Message);
    }

    [Test]
    public void ListFiles_TestThrowsWithNullPrivateKeyFilePath()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFile = string.Empty;

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI,
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.ListFiles(input, connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void ListFiles_TestThrowsWithNullPrivateKeyString()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyString = string.Empty;

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI,
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.ListFiles(input, connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }
}
