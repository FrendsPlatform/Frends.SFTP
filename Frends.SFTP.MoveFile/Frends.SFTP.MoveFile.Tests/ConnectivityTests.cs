using System;
using System.IO;
using NUnit.Framework;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile.Tests;

[TestFixture]
public class ConnectivityTests : MoveFileTestBase
{
    [Test]
    public void MoveFile_TestWithPrivateKeyFileRsa()
    {
        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        _connection.PrivateKeyFilePassphrase = "passphrase";
        _connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = SFTP.MoveFile(_input, _connection, default);
        Assert.IsNotNull(result.Files);
    }

    [Test]
    public void MoveFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        _connection.PrivateKeyFilePassphrase = "passphrase";
        _connection.PrivateKeyString = key;

        var result = SFTP.MoveFile(_input, _connection, default);
        Assert.IsNotNull(result.Files);
    }
}

