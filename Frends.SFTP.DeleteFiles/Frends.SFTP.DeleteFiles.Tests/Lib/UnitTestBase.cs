namespace Frends.SFTP.DeleteFiles.Tests;

using Frends.SFTP.DeleteFiles.Definitions;
using NUnit.Framework;

public class UnitTestBase
{
    protected static Connection _connection;
    protected static Input _input;
    protected static Options _options;

    [SetUp]
    public void Setup()
    {
        _connection = Helpers.GetSftpConnection();
        _input = new Input
        {
            Directory = "/upload",
            FileMask = string.Empty,
        };
        _options = new Options();

        Helpers.GenerateDummyFiles();
    }

    [TearDown]
    public void TearDown()
    {
        Helpers.DeleteTestFiles();
    }
}
