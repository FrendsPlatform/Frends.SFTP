using NUnit.Framework;
using Frends.SFTP.WriteFile.Definitions;
using Frends.SFTP.WriteFile.Enums;

namespace Frends.SFTP.WriteFile.Tests;

[TestFixture]
class WriteTests : WriteFileTestBase
{
    [Test]
    public void WriteFile_TestSimpleWrite()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var result = SFTP.WriteFile(input, connection);
        Assert.AreEqual("/write/test.txt", result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));
        Assert.AreEqual("test", Helpers.GetDestinationFileContent(input.Path));
    }

    [Test]
    public void WriteFile_TestWriteWithAppend()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        SFTP.WriteFile(input, connection);
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));

        input.Content = "test";
        input.WriteBehaviour = WriteOperation.Append;

        SFTP.WriteFile(input, connection);
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));
        Assert.AreEqual("test\ntest", Helpers.GetDestinationFileContent(input.Path));
    }

    [Test]
    public void WriteFile_TestWriteWithOverwrite()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Overwrite
        };

        var result = SFTP.WriteFile(input, connection);
        Assert.AreEqual("/write/test.txt", result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));
        Assert.AreEqual("test", Helpers.GetDestinationFileContent(input.Path));

        input.Content = "something else";
        SFTP.WriteFile(input, connection);
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));
        Assert.AreEqual("something else", Helpers.GetDestinationFileContent(input.Path));
    }

    [Test]
    public void WriteFile_TestWithEmptyContent()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Error
        };

        var result = SFTP.WriteFile(input, connection);
        Assert.AreEqual("/write/test.txt", result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(input.Path));
        Assert.AreEqual("", Helpers.GetDestinationFileContent(input.Path));
    }

    [Test]
    public void WriteFile_TestWithDifferentEncoding()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Path = "/write/test.txt",
            Content = "test",
            FileEncoding = FileEncoding.ANSI,
            WriteBehaviour = WriteOperation.Overwrite
        };

        var result = SFTP.WriteFile(input, connection);
        Assert.AreEqual("/write/test.txt", result.Path);
        
        input.FileEncoding = FileEncoding.ASCII;
        result = SFTP.WriteFile(input, connection);
        Assert.AreEqual("/write/test.txt", result.Path);

        input.FileEncoding = FileEncoding.UTF8;
        input.EnableBom = true;
        result = SFTP.WriteFile(input, connection);
        Assert.AreEqual("/write/test.txt", result.Path);

        input.FileEncoding = FileEncoding.UTF8;
        input.EnableBom = false;
        result = SFTP.WriteFile(input, connection);
        Assert.AreEqual("/write/test.txt", result.Path);

        input.FileEncoding = FileEncoding.WINDOWS1252;
        result = SFTP.WriteFile(input, connection);
        Assert.AreEqual("/write/test.txt", result.Path);

        input.FileEncoding = FileEncoding.Other;
        input.EncodingInString = "utf-8";
        result = SFTP.WriteFile(input, connection);
        Assert.AreEqual("/write/test.txt", result.Path);
    }
}

