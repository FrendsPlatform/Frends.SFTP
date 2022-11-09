using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Frends.SFTP.ListFiles.Definitions;

namespace Frends.SFTP.ListFiles.Tests;

[TestFixture]
public class ConnectivityTests : ListFilesTestBase
{
    [Test]
    public void ListFile_TestWithPrivateKeyFileRsa()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI
        };
        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public void ListFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyString = key;

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI
        };
        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public void ListFiles_TestWithInteractiveKeyboardAuthentication()
    {
        var connection = Helpers.GetSftpConnection();
        connection.UseKeyboardInteractiveAuthentication = true;

        var result = SFTP.ListFiles(_input, connection, new CancellationToken());
        Assert.That(result.FileCount, Is.EqualTo(3));
    }
}

