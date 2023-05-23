using System;
using System.IO;
using NUnit.Framework;
using Frends.SFTP.RenameFile.Enums;

namespace Frends.SFTP.RenameFile.Tests;

[TestFixture]
public class ConnectivityTests : RenameFileTestBase
{
    [Test]
    public void RenameFile_TestWithPrivateKeyFileRsa()
    {
        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        _connection.PrivateKeyFilePassphrase = "passphrase";
        _connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = SFTP.RenameFile(_input, _connection);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public void RenameFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        _connection.PrivateKeyFilePassphrase = "passphrase";
        _connection.PrivateKeyString = key;

        var result = SFTP.RenameFile(_input, _connection);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }
}

