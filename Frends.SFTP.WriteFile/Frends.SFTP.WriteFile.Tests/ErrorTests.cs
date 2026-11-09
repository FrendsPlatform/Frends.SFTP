using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Frends.SFTP.WriteFile.Enums;
using NUnit.Framework;
using Renci.SshNet.Common;

namespace Frends.SFTP.WriteFile.Tests;

[TestFixture]
public class ErrorTests : WriteFileTestBase
{
    [Test]
    public void WriteFile_TestFileExistsThrows()
    {
        var result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(TestInput, TestConnection, TestOptions));
        Assert.AreEqual($"File already exists: {TestInput.Path}", ex.Message);
    }

    [Test]
    public void WriteFile_TestThrowsWithWrongPort()
    {
        TestConnection.Port = 51644;
        var ex = Assert.Throws<SocketException>(() => SFTP.WriteFile(TestInput, TestConnection, TestOptions));
    }

    [Test]
    public void WriteFile_ReturnsErrorWhenThrowErrorOnFailureIsDisabled()
    {
        TestConnection.Port = 51644;
        TestOptions.ThrowErrorOnFailure = false;

        var result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNotEmpty(result.Error.Message);
        Assert.IsNotNull(result.Error.AdditionalInfo);
    }

    [Test]
    public void WriteFile_ThrowsWhenCancellationIsRequested()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            SFTP.WriteFile(TestInput, TestConnection, TestOptions, cancellationTokenSource.Token));
    }

    [Test]
    public void WriteFile_TestThrowsWithIncorrectCredentials()
    {
        TestConnection.Password = "demo";
        TestConnection.Username = "demo";

        var ex = Assert.Throws<SshAuthenticationException>(() => SFTP.WriteFile(TestInput, TestConnection, TestOptions));
        Assert.AreEqual("Permission denied (password).", ex.Message);
    }

    [Test]
    public void WriteFile_TestThrowsWithIncorrectPrivateKeyPassphrase()
    {
        TestConnection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        TestConnection.PrivateKeyPassphrase = "demo";
        TestConnection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(TestInput, TestConnection, TestOptions));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info:"));
    }

    [Test]
    public void WriteFile_TestThrowsWithEmptyPrivateKeyFile()
    {
        TestConnection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        TestConnection.PrivateKeyPassphrase = "passphrase";
        TestConnection.PrivateKeyFile = string.Empty;

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(TestInput, TestConnection, TestOptions));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void WriteFile_TestThrowsWithIncorrectPrivateKeyString()
    {
        var key = Helpers.GenerateDummySshKey();

        TestConnection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        TestConnection.PrivateKeyPassphrase = "passphrase";
        TestConnection.PrivateKeyString = key.ToString();

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(TestInput, TestConnection, TestOptions));
        Assert.IsTrue(ex.Message.StartsWith("Error when initializing connection info: "));
    }

    [Test]
    public void WriteFile_TestThrowsWithIncorrectServerFingerprint()
    {
        var fingerprint = "f6:fc:1c:03:17:5f:67:4f:1f:0b:50:5a:9f:f9:30:e5";

        TestConnection.ServerFingerPrint = fingerprint;

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(TestInput, TestConnection, TestOptions));
        Assert.IsTrue(ex.Message.StartsWith("Error when checking the server fingerprint:"), ex.Message);
    }
}
