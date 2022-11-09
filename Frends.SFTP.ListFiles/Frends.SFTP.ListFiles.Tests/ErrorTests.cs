﻿using NUnit.Framework;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Frends.SFTP.ListFiles.Definitions;

namespace Frends.SFTP.ListFiles.Tests;

[TestFixture]
public class ErrorTests : ListFilesTestBase
{
    [Test]
    public void ListFiles_TestDirectoryNotExistsThrows()
    {
        var input = new Input
        {
            Directory = "/upload/nonexisting/directory",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI
        };
            

        var ex = Assert.Throws<SftpPathNotFoundException>(() => SFTP.ListFiles(input, _connection, new CancellationToken()));
        Assert.AreEqual($"No such file", ex.Message);

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
            FileEncoding = FileEncoding.ANSI
        };

        Assert.Throws<SocketException>(() => SFTP.ListFiles(input, connection, new CancellationToken()));
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
            FileEncoding = FileEncoding.ANSI
        };

        var ex = Assert.Throws<SshAuthenticationException>(() => SFTP.ListFiles(input, connection, new CancellationToken()));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public void ListFiles_TestThrowsWithIncorrectPrivateKeyPassphrase()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "demo";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.ListFiles(input, connection, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("Error when initializing connection info:"));
    }

    [Test]
    public void ListFiles_TestThrowsWithEmptyPrivateKeyFile()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyFile = "";

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.ListFiles(input, connection, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void ListFiles_TestThrowsWithIncorrectPrivateKeyString()
    {
        var key = Helpers.GenerateDummySshKey();

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyString = key.ToString();

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.ListFiles(input, connection, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("Error when initializing connection info: "));
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
            FileEncoding = FileEncoding.ANSI
        };

        var ex = Assert.Throws<SshConnectionException>(() => SFTP.ListFiles(input, connection, new CancellationToken()));
        Assert.AreEqual("Key exchange negotiation failed.", ex.Message);
    }

    [Test]
    public void ListFiles_TestThrowsWithNullPrivateKeyFilePath()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFile = "";

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.ListFiles(input, connection, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void ListFiles_TestThrowsWithNullPrivateKeyString()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyString = "";

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.ListFiles(input, connection, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("Error when initializing connection info: "));
    }
}

