using NUnit.Framework;
using Frends.SFTP.WriteFile.Enums;
using Frends.SFTP.WriteFile.Definitions;

namespace Frends.SFTP.WriteFile.Tests;

[TestFixture]
class WriteTests : WriteFileTestBase
{
    [Test]
    public void WriteFile_TestSimpleWrite()
    {
        var result = SFTP.WriteFile(_input, _connection, _options);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(_content, Helpers.GetDestinationFileContent(_input.Path));
    }

    [Test]
    public void WriteFile_TestWriteWithAppend()
    {
        SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));

        _input.Content = "This is another line in the test file.";
        _input.WriteBehaviour = WriteOperation.Append;
        _input.AddNewLine = true;

        SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(_content + "\n" + _input.Content, Helpers.GetDestinationFileContent(_input.Path));
    }

    [Test]
    public void WriteFile_TestWriteWithAppendWithoutNewLine()
    {
        SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));

        _input.Content = "This is another line in the test file.";
        _input.WriteBehaviour = WriteOperation.Append;

        SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(_content + _input.Content, Helpers.GetDestinationFileContent(_input.Path));
    }

    [Test]
    public void WriteFile_TestWriteWithOverwrite()
    {
        _input.WriteBehaviour = WriteOperation.Overwrite;

        var result = SFTP.WriteFile(_input, _connection, _options);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(_content, Helpers.GetDestinationFileContent(_input.Path));

        _input.Content = "Something else.";
        SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(_input.Content, Helpers.GetDestinationFileContent(_input.Path));
    }

    [Test]
    public void WriteFile_TestWithEmptyContent()
    {
        _input.Content = "";

        var result = SFTP.WriteFile(_input, _connection, _options);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(string.Empty, Helpers.GetDestinationFileContent(_input.Path));
    }

    [Test]
    public void WriteFile_TestWithDifferentEncoding()
    {
        _input.WriteBehaviour = WriteOperation.Overwrite;

        var result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));


        _input.FileEncoding = FileEncoding.ASCII;
        result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = true;
        result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = false;
        result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));

        _input.FileEncoding = FileEncoding.WINDOWS1252;
        result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));

        _input.FileEncoding = FileEncoding.Other;
        _input.EncodingInString = "iso-8859-1";
        result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public void WriteFile_TestCreateDestinationDirectories_Success()
    {
        _input.Path = "/upload/new/nested/directory/testfile.txt";
        _input.Content = "Test content for new directory";
        var options = new Options { CreateDestinationDirectories = true };

        var result = SFTP.WriteFile(_input, _connection, options);

        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(_input.Content, Helpers.GetDestinationFileContent(_input.Path));
    }
}

