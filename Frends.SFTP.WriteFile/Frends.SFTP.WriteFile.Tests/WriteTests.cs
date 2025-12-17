using NUnit.Framework;
using Frends.SFTP.WriteFile.Enums;
using Frends.SFTP.WriteFile.Definitions;
using System;
using System.Text;

namespace Frends.SFTP.WriteFile.Tests;

[TestFixture]
class WriteTests : WriteFileTestBase
{
    [Test]
    public void WriteFile_TestSimpleWrite()
    {
        var result = SFTP.WriteFile(_input, _connection, _options);
        Assert.AreEqual(_input.Path, result.RemotePath);
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
        Assert.AreEqual(_input.Path, result.RemotePath);
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
        Assert.AreEqual(_input.Path, result.RemotePath);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(string.Empty, Helpers.GetDestinationFileContent(_input.Path));
    }

    [Test]
    public void WriteFile_TestWithDifferentEncoding()
    {
        _input.WriteBehaviour = WriteOperation.Overwrite;

        var result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.RemotePath));


        _input.FileEncoding = FileEncoding.ASCII;
        result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.RemotePath));

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = true;
        result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.RemotePath));

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = false;
        result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.RemotePath));

        _input.FileEncoding = FileEncoding.WINDOWS1252;
        result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.RemotePath));

        _input.FileEncoding = FileEncoding.Other;
        _input.EncodingInString = "iso-8859-1";
        result = SFTP.WriteFile(_input, _connection, _options);
        Assert.IsTrue(Helpers.DestinationFileExists(result.RemotePath));
    }

    [TestCase("/upload/new/nested/directory/testfile.txt", true, "Nested directories")]
    [TestCase("/upload/singlelevel/testfile.txt", true, "Single level")]
    [TestCase("/upload/testfile.txt", true, "Existing directory with flag true")]
    [TestCase("/upload/testfile.txt", false, "Existing directory with flag false")]
    public void WriteFile_TestCreateDestinationDirectories(string path, bool createDirs, string description)
    {
        _input.Path = path;
        _input.Content = $"Test content for {description}";
        var options = new Options { CreateDestinationDirectories = createDirs };

        var result = SFTP.WriteFile(_input, _connection, options);

        Assert.AreEqual(_input.Path, result.RemotePath);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
    }

    [Test]
    public void WriteFile_TestCreateDestinationDirectories_ThrowsWhenDisabled()
    {
        _input.Path = "/upload/nonexistent/testfile.txt";
        _input.Content = "Test content";
        var options = new Options { CreateDestinationDirectories = false };

        var ex = Assert.Throws<ArgumentException>(() =>
            SFTP.WriteFile(_input, _connection, options));

        Assert.IsTrue(ex.Message.Contains("Destination directory"));
        Assert.IsTrue(ex.Message.Contains("was not found"));
    }

    [Test]
    public void WriteFile_TestVerifyWriteEnabled()
    {
        _options.VerifyWrite = true;
        _input.Content = new string('a', 1024 * 1024);

        var result = SFTP.WriteFile(_input, _connection, _options);

        Assert.AreEqual(_input.Path, result.RemotePath);
        Assert.IsTrue(result.Verified);
        Assert.Greater(result.SizeInMegaBytes, 0d);
    }

    [Test]
    public void WriteFile_TestVerifyWriteDisabled()
    {
        _options.VerifyWrite = false;
        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = false;
        _input.Content = new string('a', 1024 * 1024);

        var result = SFTP.WriteFile(_input, _connection, _options);

        var encoding = new UTF8Encoding(false);
        var expectedBytes = encoding.GetByteCount(_input.Content);
        Assert.AreEqual(_input.Path, result.RemotePath);
        Assert.IsFalse(result.Verified);
        Assert.IsTrue(Helpers.DestinationFileExists(_input.Path));
        Assert.AreEqual(expectedBytes, Helpers.GetDestinationFileSize(_input.Path));
        Assert.AreEqual(_input.Content, Helpers.GetDestinationFileContent(_input.Path));
    }

    [Test]
    public void WriteFile_TestVerifyWriteDisabled_AppendsWithNewLine_ReturnsAppendedSize()
    {
        _options.VerifyWrite = false;
        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = false;
        var initialContent = new string('a', 1024 * 1024);
        _input.Content = initialContent;

        SFTP.WriteFile(_input, _connection, _options);

        var appendedContent = new string('b', 1024 * 1024);
        _input.Content = appendedContent;
        _input.WriteBehaviour = WriteOperation.Append;
        _input.AddNewLine = true;

        var result = SFTP.WriteFile(_input, _connection, _options);

        var encoding = new UTF8Encoding(false);
        var expectedAppendedBytes = encoding.GetByteCount("\n" + appendedContent);
        var expectedTotalBytes = encoding.GetByteCount(initialContent + "\n" + appendedContent);

        Assert.AreEqual(_input.Path, result.RemotePath);
        Assert.IsFalse(result.Verified);
        Assert.AreEqual(SizeInMegaBytes(expectedAppendedBytes), result.SizeInMegaBytes);
        Assert.AreEqual(expectedTotalBytes, Helpers.GetDestinationFileSize(_input.Path));
        Assert.AreEqual(initialContent + "\n" + appendedContent, Helpers.GetDestinationFileContent(_input.Path));
    }

    [Test]
    public void WriteFile_TestVerifyWriteDisabled_OverwriteWithBom_ReturnsSizeIncludingBom()
    {
        _options.VerifyWrite = false;
        _input.WriteBehaviour = WriteOperation.Overwrite;
        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = true;
        _input.Content = new string('a', 1024 * 1024);

        var result = SFTP.WriteFile(_input, _connection, _options);

        var encoding = new UTF8Encoding(true);
        var expectedBytes = encoding.GetPreamble().Length + encoding.GetByteCount(_input.Content);

        Assert.AreEqual(_input.Path, result.RemotePath);
        Assert.IsFalse(result.Verified);
        Assert.AreEqual(SizeInMegaBytes(expectedBytes), result.SizeInMegaBytes);
        Assert.AreEqual(expectedBytes, Helpers.GetDestinationFileSize(_input.Path));
    }

    private static double SizeInMegaBytes(long bytes) => Math.Round(bytes / (1024d * 1024d), 3);
}
