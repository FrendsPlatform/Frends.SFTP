using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.MoveFile.Definitions;
using Frends.SFTP.MoveFile.Enums;
using NUnit.Framework;

namespace Frends.SFTP.MoveFile.Tests;

[TestFixture]
internal class ConnectivityTests : MoveFileTestBase
{
    [Test]
    public async Task MoveFile_TestWithPrivateKeyFileRsa()
    {
        TestConnection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        TestConnection.PrivateKeyPassphrase = "passphrase";
        TestConnection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public async Task MoveFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        TestConnection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        TestConnection.PrivateKeyPassphrase = "passphrase";
        TestConnection.PrivateKeyString = key;

        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public async Task MoveFile_TestWithInteractiveKeyboardAuthentication()
    {
        var connection = Helpers.GetSftpConnection();
        connection.UseKeyboardInteractiveAuthentication = true;

        var result = await SFTP.MoveFile(TestInput, connection, TestOptions, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public async Task MoveFile_TestWithKeyboardInteractiveAdditionalPrompts()
    {
        TestConnection.Authentication = AuthenticationType.UsernamePassword;
        TestConnection.UseKeyboardInteractiveAuthentication = true;
        TestConnection.Password = string.Empty;
        TestConnection.PromptAndResponse = new PromptResponse[]
        {
            new() { Prompt = "password", Response = "pass" },
        };

        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, CancellationToken.None);
        Assert.IsNotNull(result.Files);
    }
}
