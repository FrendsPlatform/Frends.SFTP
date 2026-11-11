using Frends.SFTP.ReadFile.Definitions;
using Frends.SFTP.ReadFile.Enums;
using NUnit.Framework;

namespace Frends.SFTP.ReadFile.Tests.Lib;

public class ReadFileTestBase
{
    protected static Connection Connection { get; set; }

    protected static Input Input { get; set; }

    protected static Options Options { get; set; }

    protected static string Content { get; set; }

    [SetUp]
    public void SetUp()
    {
        Content = "This is a test file.";
        Connection = Helpers.GetSftpConnection();
        Input = new Input
        {
            Path = "/upload/test.txt",
            FileEncoding = FileEncoding.ANSI,
        };
        Options = new Options
        {
            ContentType = ContentType.Text,
        };

        Helpers.GenerateDummyFile(Input.Path, Content);
    }

    [TearDown]
    public void TearDown()
    {
        Helpers.DeleteSourceFile(Input.Path);
    }
}
