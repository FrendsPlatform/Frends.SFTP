using System;
using System.IO;
using Frends.SFTP.WriteFile.Enums;
using NUnit.Framework;

namespace Frends.SFTP.WriteFile.Tests;

[TestFixture]
public class ConnectivityTests : WriteFileTestBase
{
    [Test]
    public void WriteFile_TestWithLargerBuffer()
    {
        TestConnection.BufferSize = 256;

        SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
        Assert.AreEqual(TestContent, Helpers.GetDestinationFileContent(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestWithPrivateKeyFileRsa()
    {
        TestConnection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        TestConnection.PrivateKeyPassphrase = "passphrase";
        TestConnection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
        Assert.AreEqual(TestContent, Helpers.GetDestinationFileContent(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        TestConnection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        TestConnection.PrivateKeyPassphrase = "passphrase";
        TestConnection.PrivateKeyString = key;

        SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
        Assert.AreEqual(TestContent, Helpers.GetDestinationFileContent(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestWithInteractiveKeyboardAuthentication()
    {
        TestConnection.UseKeyboardInteractiveAuthentication = true;

        SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
        Assert.AreEqual(TestContent, Helpers.GetDestinationFileContent(TestInput.Path));
    }
}
