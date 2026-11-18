using System.IO;
using Frends.SFTP.RenameFile.Definitions;
using Frends.SFTP.RenameFile.Enums;
using NUnit.Framework;
using Renci.SshNet;

namespace Frends.SFTP.RenameFile.Tests;

public class RenameFileTestBase
{
    private const string DockerAddress = "localhost";
    private const string DockerUsername = "foo";
    private const string DockerPassword = "pass";

    protected static Connection TestConnection { get; private set; }

    protected static Input TestInput { get; private set; }

    protected static Options TestOptions { get; private set; }

    [SetUp]
    public void SetUp()
    {
        TestConnection = Helpers.GetSftpConnection();
        TestOptions = new Options();
        TestInput = new Input
        {
            Path = "/upload/test.txt",
            NewFileName = "example.txt",
            RenameBehaviour = RenameBehaviour.Throw,
        };

        Helpers.GenerateDummyFile(TestInput.Path);
    }

    [TearDown]
    public void TearDown()
    {
        using var client = new SftpClient(DockerAddress, 2222, DockerUsername, DockerPassword);
        client.Connect();
        Helpers.DeleteSourceFiles(client, Path.GetDirectoryName(TestInput.Path).Replace("\\", "/"));
        client.Dispose();
        client.Dispose();
    }
}
