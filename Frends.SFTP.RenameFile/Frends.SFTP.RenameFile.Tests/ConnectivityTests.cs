using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Frends.SFTP.RenameFile.Enums;

namespace Frends.SFTP.RenameFile.Tests;

[TestFixture]
public class ConnectivityTests : RenameFileTestBase
{
    [Test]
    public async Task RenameFile_TestWithPrivateKeyFileRsa()
    {
        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        _connection.PrivateKeyPassphrase = "passphrase";
        _connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        _connection.PrivateKeyPassphrase = "passphrase";
        _connection.PrivateKeyString = key;

        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestWithKeyboardInteractiveAuthentication()
    {
        _connection.Authentication = AuthenticationType.UsernamePassword;
        _connection.UseKeyboardInteractiveAuthentication = true;

        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }
}

