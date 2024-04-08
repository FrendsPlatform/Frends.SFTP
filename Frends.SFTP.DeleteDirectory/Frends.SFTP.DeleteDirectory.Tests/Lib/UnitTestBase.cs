namespace Frends.SFTP.DeleteDirectory.Tests;

using Frends.SFTP.DeleteDirectory.Definitions;
using Frends.SFTP.DeleteDirectory.Enums;
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
            Directory = "/upload/subDir",
        };
        _options = new Options
        {
            ThrowExceptionOnError = true,
            ThrowNotExistError = NotExistsOptions.Skip,
        };

        Helpers.GenerateDummyFiles();
    }

    [TearDown]
    public void TearDown()
    {
        Helpers.DeleteTestFiles();
    }
}