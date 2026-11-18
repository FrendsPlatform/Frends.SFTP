using Frends.SFTP.MoveFile.Definitions;
using Frends.SFTP.MoveFile.Enums;
using NUnit.Framework;
using Renci.SshNet;

namespace Frends.SFTP.MoveFile.Tests;

internal class MoveFileTestBase
{
    private static readonly string DockerAddress = "localhost";
    private static readonly string DockerUsername = "foo";
    private static readonly string DockerPassword = "pass";

    protected static Connection TestConnection { get; set; }

    protected static Input TestInput { get; set; }

    protected static Options TestOptions { get; set; }

    [SetUp]
    public void SetUp()
    {
        TestConnection = Helpers.GetSftpConnection();
        TestOptions = new Options();
        TestInput = new Input
        {
            Directory = "/upload/",
            Pattern = "test.txt",
            TargetDirectory = "upload/moved",
            CreateTargetDirectories = true,
            IfTargetFileExists = FileExistsOperation.Throw,
        };

        Helpers.GenerateDummyFile("test.txt");
    }

    [TearDown]
    public void TearDown()
    {
        using var client = new SftpClient(DockerAddress, 2222, DockerUsername, DockerPassword);
        client.Connect();
        Helpers.DeleteSourceFiles(client, TestInput.Directory);
        client.Disconnect();
    }
}
