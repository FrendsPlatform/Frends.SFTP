using NUnit.Framework;
using Frends.SFTP.ReadFile.Definitions;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile.Tests;
public class ReadFileTestBase
{
    internal static Connection _connection;
    internal static Input _input;
    internal static string _content;

    [SetUp]
    public void SetUp()
    {
        _content = "This is a test file.";
        _connection = Helpers.GetSftpConnection();
        _input = new Input
        {
            Path = "/upload/test.txt",
            FileEncoding = FileEncoding.ANSI
        };

        Helpers.GenerateDummyFile(_input.Path, _content);
    }

    [TearDown]
    public void TearDown()
    {
        Helpers.DeleteSourceFile(_input.Path);
    }
}
