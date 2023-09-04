﻿namespace Frends.SFTP.DeleteFiles.Tests;

using System;
using System.IO;
using System.Net.Sockets;
using Frends.SFTP.DeleteFiles.Definitions;
using Frends.SFTP.DeleteFiles.Enums;
using NUnit.Framework;
using Renci.SshNet.Common;

[TestFixture]
public class ConnectionTests : UnitTestBase
{
    [Test]
    public void DeleteFiles_TestTransferThatThrowsWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ConnectionTimeout = 10;
        connection.Username = "demo";
        connection.Password = "demo";

        var ex = Assert.Throws<SshAuthenticationException>(() => SFTP.DeleteFiles(_input, connection, default));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public void DeleteFiles_TestPrivateKeyFileRsa()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public void DeleteFiles_TestPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        var connection = Helpers.GetSftpConnection();
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyString = key;

        var result = SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public void DeleteFiles_TestWithInteractiveKeyboardAuthentication()
    {
        var connection = Helpers.GetSftpConnection();
        connection.UseKeyboardInteractiveAuthentication = true;

        var result = SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public void DeleteFiles_TestWithInteractiveKeyboardAuthenticationAndPrivateKey()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePrivateKeyFile;
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");
        connection.Password = null;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.UseKeyboardInteractiveAuthentication = true;
        connection.PromptAndResponse = new PromptResponse[] { new PromptResponse { Prompt = "Password", Response = "pass" } };

        var result = SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);

        connection.Authentication = AuthenticationType.UsernamePrivateKeyString;
        connection.PrivateKeyFile = null;
        connection.PrivateKeyString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));
        connection.PrivateKeyPassphrase = "passphrase";

        _input.Directory = "/delete/subDir";
        result = SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public void DeleteFiles_TestThrowsWithWrongPort()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Port = 51644;
        var input = new Input
        {
            Directory = "/upload",
            FileMask = "filenotexisting.txt",
        };

        Assert.Throws<SocketException>(() => SFTP.DeleteFiles(input, connection, default));
    }

    [Test]
    public void DeleteFiles_TestThrowsWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Password = "demo";
        connection.Username = "demo";

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "filenotexisting.txt",
        };

        var ex = Assert.Throws<SshAuthenticationException>(() => SFTP.DeleteFiles(input, connection, default));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public void DeleteFiles_TestThrowsWithIncorrectPrivateKeyPassphrase()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "demo";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "filenotexisting.txt",
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.DeleteFiles(input, connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info:"));
    }

    [Test]
    public void DeleteFiles_TestThrowsWithEmptyPrivateKeyFile()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyFile = "";

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "filenotexisting.txt",
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.DeleteFiles(input, connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void DeleteFiles_TestThrowsWithIncorrectPrivateKeyString()
    {
        var key = Helpers.GenerateDummySshKey();

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyString = key.ToString();

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "filenotexisting.txt",
        };

        var ex = Assert.Throws<ArgumentException>(() => SFTP.DeleteFiles(input, connection, default));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void DeleteFiles_TestThrowsWithIncorrectServerFingerprint()
    {
        var fingerprint = "f6:fc:1c:03:17:5f:67:4f:1f:0b:50:5a:9f:f9:30:e5";

        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = fingerprint;

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "filenotexisting.txt",
        };

        var ex = Assert.Throws<SshConnectionException>(() => SFTP.DeleteFiles(input, connection, default));
        Assert.AreEqual("Key exchange negotiation failed.", ex.Message);
    }
}
