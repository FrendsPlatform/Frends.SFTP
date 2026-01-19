using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.MoveFile.Definitions;
using NUnit.Framework;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile.Tests;

[TestFixture]
public class ConnectivityTests : MoveFileTestBase
{
    [Test]
    public async Task MoveFile_TestWithPrivateKeyFileRsa()
    {
        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        _connection.PrivateKeyPassphrase = "passphrase";
        _connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = await SFTP.MoveFile(_input, _connection, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public async Task MoveFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        _connection.PrivateKeyPassphrase = "passphrase";
        _connection.PrivateKeyString = key;

        var result = await SFTP.MoveFile(_input, _connection, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public async Task MoveFile_TestWithInteractiveKeyboardAuthentication()
    {
        var connection = Helpers.GetSftpConnection();
        connection.UseKeyboardInteractiveAuthentication = true;

        var result = await SFTP.MoveFile(_input, connection, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public async Task MoveFile_TestWithKeyboardInteractiveAdditionalPrompts()
    {
        _connection.Authentication = AuthenticationType.UsernamePassword;
        _connection.UseKeyboardInteractiveAuthentication = true;
        _connection.Password = string.Empty;
        _connection.PromptAndResponse = new PromptResponse[]
        {
            new() { Prompt = "password", Response = "pass" },
        };

        var result = await SFTP.MoveFile(_input, _connection, CancellationToken.None);
        Assert.IsNotNull(result.Files);
    }
}