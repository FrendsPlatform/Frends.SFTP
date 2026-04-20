namespace Frends.SFTP.DeleteDirectory.Tests;

using Frends.SFTP.DeleteDirectory.Definitions;
using Frends.SFTP.DeleteDirectory.Enums;
using NUnit.Framework;

public class UnitTestBase
{
    protected Connection _connection;
    protected Input _input;
    protected Options _options;

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