using Frends.SFTP.WriteFile.Definitions;
using Frends.SFTP.WriteFile.Enums;
using NUnit.Framework;

namespace Frends.SFTP.WriteFile.Tests;

public class WriteFileTestBase
{
    internal static Input TestInput { get; set; }

    internal static Connection TestConnection { get; set; }

    internal static Options TestOptions { get; set; }

    internal static string TestContent { get; set; }

    [SetUp]
    public void SetUp()
    {
        TestContent = "This is a test file.";
        TestConnection = Helpers.GetSftpConnection();
        TestInput = new Input
        {
            Path = "/upload/test.txt",
            Content = TestContent,
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error,
        };
        TestOptions = new Options();
    }

    [TearDown]
    public void TearDown()
    {
        Helpers.DeleteDestinationFiles();
    }
}
