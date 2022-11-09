using System;
using System.IO;
using NUnit.Framework;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile.Tests;

[TestFixture]
public class ConnectivityTests : ReadFileTestBase
{
    [Test]
    public void ReadFile_TestWithLargerBuffer()
    {
        _connection.BufferSize = 256;

        var result = SFTP.ReadFile(_input, _connection);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public void ReadFile_TestWithPrivateKeyFileRsa()
    {
        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        _connection.PrivateKeyFilePassphrase = "passphrase";
        _connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = SFTP.ReadFile(_input, _connection);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public void ReadFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        _connection.PrivateKeyFilePassphrase = "passphrase";
        _connection.PrivateKeyString = key;

        var result = SFTP.ReadFile(_input, _connection);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.AreEqual(_content, result.Content);
    }
}

