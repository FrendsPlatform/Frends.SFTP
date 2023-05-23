using NUnit.Framework;
using Frends.SFTP.RenameFile.Definitions;
using Frends.SFTP.RenameFile.Enums;
using System.IO;
using Renci.SshNet;

namespace Frends.SFTP.RenameFile.Tests;
public class RenameFileTestBase
{
    internal static Connection _connection;
    internal static Input _input;

    /// <summary>
    /// Test credentials for docker server.
    /// </summary>
    readonly static string _dockerAddress = "localhost";
    readonly static string _dockerUsername = "foo";
    readonly static string _dockerPassword = "pass";

    [SetUp]
    public void SetUp()
    {
        _connection = Helpers.GetSftpConnection();
        _input = new Input
        {
            Path = "/upload/test.txt",
            NewFileName = "example.txt",
            RenameBehaviour = RenameBehaviour.Throw
        };

        Helpers.GenerateDummyFile(_input.Path);
    }

    [TearDown]
    public void TearDown()
    {
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        Helpers.DeleteSourceFiles(client, Path.GetDirectoryName(_input.Path).Replace("\\", "/"));
        client.Dispose();
        client.Dispose();
    }
}
