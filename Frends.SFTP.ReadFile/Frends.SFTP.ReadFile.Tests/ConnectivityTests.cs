using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.ReadFile.Definitions;
using NUnit.Framework;
using Frends.SFTP.ReadFile.Enums;
using Frends.SFTP.ReadFile.Tests.Lib;

namespace Frends.SFTP.ReadFile.Tests;

[TestFixture]
public class ConnectivityTests : ReadFileTestBase
{
    [Test]
    public async Task ReadFile_TestWithLargerBuffer()
    {
        Connection.BufferSize = 256;

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Input.Path, result.Path);
        Assert.AreEqual(Content, result.TextContent);
    }

    [Test]
    public async Task ReadFile_TestWithPrivateKeyFileRsa()
    {
        Connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        Connection.PrivateKeyPassphrase = "passphrase";
        Connection.PrivateKeyFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Input.Path, result.Path);
        Assert.AreEqual(Content, result.TextContent);
    }

    [Test]
    public async Task ReadFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "../../../Volumes/ssh_host_rsa_key"));

        Connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        Connection.PrivateKeyPassphrase = "passphrase";
        Connection.PrivateKeyString = key;

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Input.Path, result.Path);
        Assert.AreEqual(Content, result.TextContent);
    }

    [Test]
    public async Task ReadFile_TestWithKeyboardInteractive()
    {
        Connection.Authentication = AuthenticationType.UsernamePassword;
        Connection.UseKeyboardInteractiveAuthentication = true;

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Input.Path, result.Path);
        Assert.AreEqual(Content, result.TextContent);
    }

    [Test]
    public async Task ReadFile_TestWithKeyboardInteractiveAdditionalPrompts()
    {
        Connection.Authentication = AuthenticationType.UsernamePassword;
        Connection.UseKeyboardInteractiveAuthentication = true;
        Connection.Password = string.Empty;
        Connection.PromptAndResponse = new PromptResponse[]
        {
            new()
            {
                Prompt = "password",
                Response = "pass"
            },
        };

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Input.Path, result.Path);
        Assert.AreEqual(Content, result.TextContent);
    }
}
