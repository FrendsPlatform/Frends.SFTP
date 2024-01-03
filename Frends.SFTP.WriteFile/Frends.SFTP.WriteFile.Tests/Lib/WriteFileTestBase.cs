using NUnit.Framework;
using Frends.SFTP.WriteFile.Definitions;
using Frends.SFTP.WriteFile.Enums;

namespace Frends.SFTP.WriteFile.Tests;

public class WriteFileTestBase
{
    internal static Input _input;
    internal static Connection _connection;
    internal static string _content;

    [SetUp]
    public void SetUp()
    {
        _content = "This is a test file.";
        _connection = Helpers.GetSftpConnection();
        _input = new Input
        {
            Path = "/upload/test.txt",
            Content = _content,
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };
    }

    [TearDown]
    public void TearDown()
    {
        Helpers.DeleteDestinationFiles();
    }
}

