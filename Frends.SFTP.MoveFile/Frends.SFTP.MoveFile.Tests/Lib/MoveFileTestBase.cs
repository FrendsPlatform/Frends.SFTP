using NUnit.Framework;
using Renci.SshNet;
using Frends.SFTP.MoveFile.Definitions;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile.Tests;
public class MoveFileTestBase
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
            Directory = "/upload/",
            Pattern = "test.txt",
            TargetDirectory = "upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw
        };

        Helpers.GenerateDummyFile("test.txt");
    }

    [TearDown]
    public void TearDown()
    {
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        Helpers.DeleteSourceFiles(client, _input.Directory);
        client.Disconnect();
    }
}
