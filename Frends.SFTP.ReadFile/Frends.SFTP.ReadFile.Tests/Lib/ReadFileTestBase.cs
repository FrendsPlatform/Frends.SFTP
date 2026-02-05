using Frends.SFTP.ReadFile.Definitions;
using Frends.SFTP.ReadFile.Enums;
using NUnit.Framework;

namespace Frends.SFTP.ReadFile.Tests.Lib;

public class ReadFileTestBase
{
    internal static Connection Connection;
    internal static Input Input;
    internal static Options Options;
    internal static string Content;

    [SetUp]
    public void SetUp()
    {
        Content = "This is a test file.";
        Connection = Helpers.GetSftpConnection();
        Input = new Input
        {
            Path = "/upload/test.txt",
            FileEncoding = FileEncoding.ANSI
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
