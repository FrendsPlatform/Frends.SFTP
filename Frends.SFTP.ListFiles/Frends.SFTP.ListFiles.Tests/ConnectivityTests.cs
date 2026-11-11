using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.ListFiles.Definitions;
using Frends.SFTP.ListFiles.Enums;
using Frends.SFTP.ListFiles.Tests.Lib;
using NUnit.Framework;

namespace Frends.SFTP.ListFiles.Tests;

[TestFixture]
public class ConnectivityTests : ListFilesTestBase
{
    [Test]
    public async Task ListFile_TestWithPrivateKeyFileRsa()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI,
        };
        var result = await SFTP.ListFiles(input, connection, Options, CancellationToken.None);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public async Task ListFile_TestWithPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        var connection = Lib.Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyPassphrase = "passphrase";
        connection.PrivateKeyString = key;

        var input = new Input
        {
            Directory = "/upload",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI,
        };
        var result = await SFTP.ListFiles(input, connection, Options, CancellationToken.None);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public async Task ListFiles_TestWithInteractiveKeyboardAuthentication()
    {
        var connection = Lib.Helpers.GetSftpConnection();
        connection.UseKeyboardInteractiveAuthentication = true;

        var result = await SFTP.ListFiles(Input, connection, Options, CancellationToken.None);
        Assert.AreEqual(3, result.FileCount);
    }
}
