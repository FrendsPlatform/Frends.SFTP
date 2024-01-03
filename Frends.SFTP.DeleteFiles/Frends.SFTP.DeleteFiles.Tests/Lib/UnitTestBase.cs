namespace Frends.SFTP.DeleteFiles.Tests;

using Frends.SFTP.DeleteFiles.Definitions;
using NUnit.Framework;

public class UnitTestBase
{
    protected static Connection _connection;
    protected static Input _input;

    [SetUp]
    public void Setup()
    {
        _connection = Helpers.GetSftpConnection();
        _input = new Input
        {
            Directory = "/upload",
            FileMask = string.Empty,
        };

        Helpers.GenerateDummyFiles();
    }

    [TearDown]
    public void TearDown()
    {
        Helpers.DeleteTestFiles();
    }
}