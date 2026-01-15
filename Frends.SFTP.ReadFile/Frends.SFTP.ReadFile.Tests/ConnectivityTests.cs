using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.ReadFile.Definitions;
using NUnit.Framework;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile.Tests;

[TestFixture]
public class ConnectivityTests : ReadFileTestBase
{
    [Test]
    public async Task ReadFile_TestWithLargerBuffer()
    {
        _connection.BufferSize = 256;

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public async Task ReadFile_TestWithPrivateKeyFileRsa()
    {
        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        _connection.PrivateKeyPassphrase = "passphrase";
        _connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public async Task ReadFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        _connection.PrivateKeyPassphrase = "passphrase";
        _connection.PrivateKeyString = key;

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public async Task ReadFile_TestWithKeyboardinteractive()
    {
        _connection.Authentication = AuthenticationType.UsernamePassword;
        _connection.UseKeyboardInteractiveAuthentication = true;

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public async Task ReadFile_TestWithKeyboardInteractiveAdditionalPrompts()
    {
        _connection.Authentication = AuthenticationType.UsernamePassword;
        _connection.UseKeyboardInteractiveAuthentication = true;
        _connection.Password = string.Empty;
        _connection.PromptAndResponse = new PromptResponse[]
        {
            new() { Prompt = "password", Response = "pass" },
        };

        var result = await SFTP.ReadFile(_input, _connection, CancellationToken.None);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.AreEqual(_content, result.Content);
    }
}