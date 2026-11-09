using System;
using System.IO;
using System.Threading.Tasks;
using Frends.SFTP.RenameFile.Enums;
using NUnit.Framework;

namespace Frends.SFTP.RenameFile.Tests;

[TestFixture]
public class ConnectivityTests : RenameFileTestBase
{
    [Test]
    public async Task RenameFile_TestWithPrivateKeyFileRsa()
    {
        TestConnection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        TestConnection.PrivateKeyPassphrase = "passphrase";
        TestConnection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        TestConnection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        TestConnection.PrivateKeyPassphrase = "passphrase";
        TestConnection.PrivateKeyString = key;

        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestWithKeyboardInteractiveAuthentication()
    {
        TestConnection.Authentication = AuthenticationType.UsernamePassword;
        TestConnection.UseKeyboardInteractiveAuthentication = true;

        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }
}
