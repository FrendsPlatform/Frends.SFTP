using System;
using Frends.SFTP.WriteFile.Definitions;
using Frends.SFTP.WriteFile.Enums;
using NUnit.Framework;

namespace Frends.SFTP.WriteFile.Tests;

[TestFixture]
public class WriteTests : WriteFileTestBase
{
    [Test]
    public void WriteFile_TestSimpleWrite()
    {
        var result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.AreEqual(TestInput.Path, result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
        Assert.AreEqual(TestContent, Helpers.GetDestinationFileContent(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestWriteWithAppend()
    {
        SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));

        TestInput.Content = "This is another line in the test file.";
        TestInput.WriteBehaviour = WriteOperation.Append;
        TestInput.AddNewLine = true;

        SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
        Assert.AreEqual(TestContent + "\n" + TestInput.Content, Helpers.GetDestinationFileContent(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestWriteWithAppendWithoutNewLine()
    {
        SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));

        TestInput.Content = "This is another line in the test file.";
        TestInput.WriteBehaviour = WriteOperation.Append;

        SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
        Assert.AreEqual(TestContent + TestInput.Content, Helpers.GetDestinationFileContent(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestWriteWithOverwrite()
    {
        TestInput.WriteBehaviour = WriteOperation.Overwrite;

        var result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.AreEqual(TestInput.Path, result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
        Assert.AreEqual(TestContent, Helpers.GetDestinationFileContent(TestInput.Path));

        TestInput.Content = "Something else.";
        SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
        Assert.AreEqual(TestInput.Content, Helpers.GetDestinationFileContent(TestInput.Path));
    }

    [Test]
    public void WriteFile_InvalidEncodingNameShouldThrowClearError()
    {
        TestInput.FileEncoding = FileEncoding.Other;
        TestInput.EncodingInString = "iso-invalid-encoding";

        var ex = Assert.Throws<ArgumentException>(() => SFTP.WriteFile(TestInput, TestConnection, TestOptions));
        Assert.IsTrue(ex.Message.Contains("Unknown encoding: 'iso-invalid-encoding'"), ex.Message);
    }

    [Test]
    public void WriteFile_Iso88591EncodingShouldWork()
    {
        TestInput.FileEncoding = FileEncoding.Other;
        TestInput.EncodingInString = "iso-8859-1";

        var result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public void WriteFile_TestWithEmptyContent()
    {
        TestInput.Content = string.Empty;

        var result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.AreEqual(TestInput.Path, result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
        Assert.AreEqual(string.Empty, Helpers.GetDestinationFileContent(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestWithDifferentEncoding()
    {
        TestInput.WriteBehaviour = WriteOperation.Overwrite;

        var result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
        TestInput.FileEncoding = FileEncoding.ASCII;
        result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));

        TestInput.FileEncoding = FileEncoding.UTF8;
        TestInput.EnableBom = true;
        result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));

        TestInput.FileEncoding = FileEncoding.UTF8;
        TestInput.EnableBom = false;
        result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));

        TestInput.FileEncoding = FileEncoding.WINDOWS1252;
        result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));

        TestInput.FileEncoding = FileEncoding.Other;
        TestInput.EncodingInString = "iso-8859-1";
        result = SFTP.WriteFile(TestInput, TestConnection, TestOptions);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [TestCase("/upload/new/nested/directory/testfile.txt", true, "Nested directories")]
    [TestCase("/upload/singlelevel/testfile.txt", true, "Single level")]
    [TestCase("/upload/testfile.txt", true, "Existing directory with flag true")]
    [TestCase("/upload/testfile.txt", false, "Existing directory with flag false")]
    public void WriteFile_TestCreateDestinationDirectories(string path, bool createDirs, string description)
    {
        TestInput.Path = path;
        TestInput.Content = $"Test content for {description}";
        var options = new Options { CreateDestinationDirectories = createDirs };

        var result = SFTP.WriteFile(TestInput, TestConnection, options);

        Assert.AreEqual(TestInput.Path, result.Path);
        Assert.IsTrue(Helpers.DestinationFileExists(TestInput.Path));
    }

    [Test]
    public void WriteFile_TestCreateDestinationDirectories_ThrowsWhenDisabled()
    {
        TestInput.Path = "/upload/nonexistent/testfile.txt";
        TestInput.Content = "Test content";
        var options = new Options { CreateDestinationDirectories = false };

        var ex = Assert.Throws<ArgumentException>(() =>
            SFTP.WriteFile(TestInput, TestConnection, options));

        Assert.IsTrue(ex.Message.Contains("Destination directory"));
        Assert.IsTrue(ex.Message.Contains("was not found"));
    }
}
