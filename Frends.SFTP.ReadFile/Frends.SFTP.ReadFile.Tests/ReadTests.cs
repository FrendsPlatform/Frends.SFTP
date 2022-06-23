using NUnit.Framework;
using Frends.SFTP.ReadFile.Definitions;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile.Tests;

[TestFixture]
class ReadTests
{
    [Test]
    public void ReadFile_TestSimpleRead()
    {
        var content = "Test";

        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Path = "/read/test.txt",
            FileEncoding = FileEncoding.ANSI,
        };

        Helpers.GenerateDummyFile(input.Path, content);

        var result = SFTP.ReadFile(input, connection);
        Assert.AreEqual(content, result.Content);

        Helpers.DeleteSourceFile(input.Path);
    }

    [Test]
    public void ReadFile_TestWithEmptyContent()
    {
        var content = "";
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Path = "/read/test.txt",
            FileEncoding = FileEncoding.ANSI,
        };

        Helpers.GenerateDummyFile(input.Path, content);

        var result = SFTP.ReadFile(input, connection);
        Assert.That(result.Content == "");

        Helpers.DeleteSourceFile(input.Path);
    }

    [Test]
    public void ReadFile_TestWithDifferentEncoding()
    {
        var content = "Test";

        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Path = "/read/test.txt",
            FileEncoding = FileEncoding.ANSI,
        };

        Helpers.GenerateDummyFile(input.Path, content);

        var result = SFTP.ReadFile(input, connection);
        Assert.AreEqual(content, result.Content);
        
        input.FileEncoding = FileEncoding.ASCII;
        result = SFTP.ReadFile(input, connection);
        Assert.AreEqual(content, result.Content);

        input.FileEncoding = FileEncoding.UTF8;
        input.EnableBom = true;
        result = SFTP.ReadFile(input, connection);
        Assert.AreEqual(content, result.Content);

        input.FileEncoding = FileEncoding.UTF8;
        input.EnableBom = false;
        result = SFTP.ReadFile(input, connection);
        Assert.AreEqual(content, result.Content);

        input.FileEncoding = FileEncoding.WINDOWS1252;
        result = SFTP.ReadFile(input, connection);
        Assert.AreEqual(content, result.Content);

        input.FileEncoding = FileEncoding.Other;
        input.EncodingInString = "utf-8";
        result = SFTP.ReadFile(input, connection);
        Assert.AreEqual(content, result.Content);

        Helpers.DeleteSourceFile(input.Path);
    }
}

