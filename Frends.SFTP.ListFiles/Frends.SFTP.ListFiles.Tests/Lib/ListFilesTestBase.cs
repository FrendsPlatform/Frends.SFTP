using Frends.SFTP.ListFiles.Definitions;
using Frends.SFTP.ListFiles.Enums;
using NUnit.Framework;

namespace Frends.SFTP.ListFiles.Tests.Lib;

public class ListFilesTestBase
{
    protected static Connection Connection { get; private set; }

    protected static Input Input { get; private set; }

    protected static Options Options { get; private set; }

    [SetUp]
    public void Setup()
    {
        Connection = Helpers.GetSftpConnection();
        Options = new Options();
        Input = new Input
        {
            Directory = "/upload",
            FileMask = string.Empty,
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI,
        };

        Helpers.GenerateDummyFiles();
    }

    [TearDown]
    public void TearDown()
    {
        Helpers.DeleteTestFiles();
    }
}
