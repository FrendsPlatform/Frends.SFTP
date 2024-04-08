namespace Frends.SFTP.DeleteDirectory.Tests;

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Frends.SFTP.DeleteDirectory.Definitions;
using Frends.SFTP.DeleteDirectory.Enums;
using NUnit.Framework;
using Renci.SshNet.Common;

[TestFixture]
public class ConnectionTests : UnitTestBase
{
    [Test]
    public void DeleteDirectory_TestTransferThatThrowsWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ConnectionTimeout = 10;
        connection.Username = "demo";
        connection.Password = "demo";

        var ex = Assert.ThrowsAsync<SshAuthenticationException>(async () => await SFTP.DeleteDirectory(_input, connection, _options, default));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public async Task DeleteDirectory_TestTransferThatReturnsErrorWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ConnectionTimeout = 10;
        connection.Username = "demo";
        connection.Password = "demo";

        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var ex = await SFTP.DeleteDirectory(_input, connection, options, default);
        Assert.IsTrue(ex.ErrorMessage.Message.Contains("Permission denied (password)."));
    }

    [Test]
    public async Task DeleteDirectory_TestPrivateKeyFileRsa()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = await SFTP.DeleteDirectory(_input, connection, _options, default);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task DeleteDirectory_TestPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        var connection = Helpers.GetSftpConnection();
        connection.HostKeyAlgorithm = HostKeyAlgorithms.RSA;
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyString = key;

        var result = await SFTP.DeleteDirectory(_input, connection, _options, default);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task DeleteDirectory_TestWithInteractiveKeyboardAuthentication()
    {
        var connection = Helpers.GetSftpConnection();
        connection.UseKeyboardInteractiveAuthentication = true;

        var result = await SFTP.DeleteDirectory(_input, connection, _options, default);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void DeleteDirectory_TestThrowsWithWrongPort()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Port = 51644;
        var input = new Input
        {
            Directory = "/upload",
        };

        Assert.ThrowsAsync<SocketException>(async () => await SFTP.DeleteDirectory(input, connection, _options, default));
    }

    [Test]
    public async Task DeleteDirectory_TestReturnsErrorWithWrongPort()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Port = 51644;
        var input = new Input
        {
            Directory = "/upload",
        };

        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var ex = await SFTP.DeleteDirectory(input, connection, options, default);
        Assert.NotNull(ex.ErrorMessage);
    }

    [Test]
    public void DeleteDirectory_TestThrowsWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Password = "demo";
        connection.Username = "demo";

        var input = new Input
        {
            Directory = "/upload",
        };

        var ex = Assert.ThrowsAsync<SshAuthenticationException>(async () => await SFTP.DeleteDirectory(input, connection, _options, default));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public async Task DeleteDirectory_TestReturnWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Password = "demo";
        connection.Username = "demo";

        var input = new Input
        {
            Directory = "/upload",
        };

        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var ex = await SFTP.DeleteDirectory(input, connection, options, default);
        Assert.IsTrue(ex.ErrorMessage.Message.Contains("Permission denied (password)."));
    }

    [Test]
    public void DeleteDirectory_TestThrowsWithIncorrectPrivateKeyPassphrase()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "demo";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Directory = "/upload",
        };

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await SFTP.DeleteDirectory(input, connection, _options, default));
        Assert.IsTrue(ex.Message.StartsWith("Invalid data type, INTEGER(02) is expected, but was 8C"));
    }

    [Test]
    public async Task DeleteDirectory_TestReturnErrorWithIncorrectPrivateKeyPassphrase()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "demo";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Directory = "/upload",
        };

        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var ex = await SFTP.DeleteDirectory(input, connection, options, default);
        Assert.IsTrue(ex.ErrorMessage.Message.Contains("Invalid data type, INTEGER(02) is expected, but was 8C"));
    }

    [Test]
    public void DeleteDirectory_TestThrowsWithEmptyPrivateKeyFile()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyFile = string.Empty;

        var input = new Input
        {
            Directory = "/upload",
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.DeleteDirectory(input, connection, _options, default));
        Assert.IsTrue(ex.Message.StartsWith("Private key file path was not given."));
    }

    [Test]
    public async Task DeleteDirectory_TestReturnErrorWithEmptyPrivateKeyFile()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyFile = string.Empty;

        var input = new Input
        {
            Directory = "/upload",
        };

        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var ex = await SFTP.DeleteDirectory(input, connection, options, default);
        Assert.IsTrue(ex.ErrorMessage.Message.Contains("Private key file path was not given."));
    }

    [Test]
    public void DeleteDirectory_TestThrowsWithIncorrectPrivateKeyString()
    {
        var key = Helpers.GenerateDummySshKey();

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyString = key.ToString();

        var input = new Input
        {
            Directory = "/upload",
        };

        var ex = Assert.ThrowsAsync<SshException>(async () => await SFTP.DeleteDirectory(input, connection, _options, default));
        Assert.IsTrue(ex.Message.StartsWith("Invalid private key file."));
    }

    [Test]
    public async Task DeleteDirectory_TestReturnsErrorWithIncorrectPrivateKeyString()
    {
        var key = Helpers.GenerateDummySshKey();

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyString = key.ToString();

        var input = new Input
        {
            Directory = "/upload",
        };

        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var ex = await SFTP.DeleteDirectory(input, connection, options, default);
        Assert.IsTrue(ex.ErrorMessage.Message.Contains("Invalid private key file."));
    }

    [Test]
    public void DeleteDirectory_TestThrowsWithIncorrectServerFingerprint()
    {
        var fingerprint = "f6:fc:1c:03:17:5f:67:4f:1f:0b:50:5a:9f:f9:30:e5";

        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = fingerprint;

        var input = new Input
        {
            Directory = "/upload",
        };

        var ex = Assert.ThrowsAsync<SshConnectionException>(async () => await SFTP.DeleteDirectory(input, connection, _options, default));
        Assert.AreEqual("Key exchange negotiation failed.", ex.Message);
    }

    [Test]
    public async Task DeleteDirectory_TestReturnsErrorWithIncorrectServerFingerprint()
    {
        var fingerprint = "f6:fc:1c:03:17:5f:67:4f:1f:0b:50:5a:9f:f9:30:e5";

        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = fingerprint;

        var input = new Input
        {
            Directory = "/upload",
        };

        var options = new Options
        {
            ThrowExceptionOnError = false,
        };

        var ex = await SFTP.DeleteDirectory(input, connection, options, default);
        Assert.IsTrue(ex.ErrorMessage.Message.Contains("Key exchange negotiation failed."));
    }
}