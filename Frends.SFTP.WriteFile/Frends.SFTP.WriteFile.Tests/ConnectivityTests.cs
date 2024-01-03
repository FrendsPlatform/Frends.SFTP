using System;
using System.IO;
using NUnit.Framework;
using Frends.SFTP.WriteFile.Enums;

namespace Frends.SFTP.WriteFile.Tests;

[TestFixture]
public class ConnectivityTests : WriteFileTestBase
{
    [Test]
    public void WriteFile_TestWithLargerBuffer()
    {
        _connection.BufferSize = 256;

        SFTP.WriteFile(_input, _connection);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(_content, Helpers.GetDestinationFileContent(_input.Path));
    }

    [Test]
    public void WriteFile_TestWithPrivateKeyFileRsa()
    {
        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        _connection.PrivateKeyPassphrase = "passphrase";
        _connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        SFTP.WriteFile(_input, _connection);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(_content, Helpers.GetDestinationFileContent(_input.Path));
    }

    [Test]
    public void WriteFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        _connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        _connection.PrivateKeyPassphrase = "passphrase";
        _connection.PrivateKeyString = key;

        SFTP.WriteFile(_input, _connection);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(_content, Helpers.GetDestinationFileContent(_input.Path));
    }

    [Test]
    public void WriteFile_TestWithInteractiveKeyboardAuthentication()
    {
        _connection.UseKeyboardInteractiveAuthentication = true;

        SFTP.WriteFile(_input, _connection);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(_content, Helpers.GetDestinationFileContent(_input.Path));
    }
}

