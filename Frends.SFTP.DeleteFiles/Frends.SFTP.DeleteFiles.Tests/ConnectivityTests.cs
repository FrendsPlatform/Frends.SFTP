namespace Frends.SFTP.DeleteFiles.Tests;

using System;
using System.IO;
using Frends.SFTP.DeleteFiles.Definitions;
using Frends.SFTP.DeleteFiles.Enums;
using NUnit.Framework;
using Renci.SshNet.Common;

[TestFixture]
public class ConnectivityTests : UnitTestBase
{
    [Test]
    public void DownloadFiles_TestTransferThatThrowsWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ConnectionTimeout = 10;
        connection.Username = "demo";
        connection.Password = "demo";

        var ex = Assert.Throws<SshAuthenticationException>(() => SFTP.DeleteFiles(_input, connection, default));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public void DownloadFiles_TestPrivateKeyFileRsa()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public void DownloadFiles_TestPrivateKeyFileRsaFromString()
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
    public void DownloadFiles_TestWithInteractiveKeyboardAuthentication()
    {
        var connection = Helpers.GetSftpConnection();
        connection.UseKeyboardInteractiveAuthentication = true;

        var result = SFTP.DeleteFiles(_input, connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public void DownloadFiles_TestWithInteractiveKeyboardAuthenticationAndPrivateKey()
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
}
