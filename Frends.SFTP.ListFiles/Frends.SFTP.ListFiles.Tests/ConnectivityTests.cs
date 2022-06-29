using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Frends.SFTP.ListFiles.Definitions;
using Frends.SFTP.ListFiles.Enums;

namespace Frends.SFTP.ListFiles.Tests;

[TestFixture]
public class ConnectivityTests : ListFilesTestBase
{
    [Test]
    public void ReadFile_TestWithMD5ServerFingerprint()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Helpers.GetServerFingerprintAsMD5String();
        var input = new Input
        {
            Directory = "/listfiles",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI
        };
        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(3, result.Count);
    }

    [Test]
    public void ReadFile_TestWithSHA256ServerFingerprint()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ServerFingerPrint = Helpers.GetServerFingerprintAsSHA256String();
        var input = new Input
        {
            Directory = "/listfiles",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI
        };
        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(3, result.Count);
    }

    [Test]
    public void ReadFile_TestWithPrivateKeyFileRsa()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Directory = "/listfiles",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI
        };
        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(3, result.Count);
    }

    [Test]
    public void ReadFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyString = key;

        var input = new Input
        {
            Directory = "/listfiles",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI
        };
        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(3, result.Count);
    }
}

