using NUnit.Framework;
using Frends.SFTP.ListFiles.Definitions;

namespace Frends.SFTP.ListFiles.Tests;

public class ListFilesTestBase
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
            FileMask = "",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI
        };

        Helpers.GenerateDummyFiles();
    }

    [TearDown]
    public void TearDown()
    {
        Helpers.DeleteTestFiles();
    }
}

