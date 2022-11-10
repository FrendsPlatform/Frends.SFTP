using NUnit.Framework;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile.Tests;

[TestFixture]
class ReadTests : ReadFileTestBase
{
    [Test]
    public void ReadFile_TestSimpleRead()
    {
        var result = SFTP.ReadFile(_input, _connection);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public void ReadFile_TestWithEmptyContent()
    {
        var content = "";

        Helpers.OverrideDummyFile(_input.Path, content);

        var result = SFTP.ReadFile(_input, _connection);
        Assert.AreEqual(string.Empty, result.Content);
    }

    [Test]
    public void ReadFile_TestWithDifferentEncoding()
    {
        _input.FileEncoding = FileEncoding.ANSI;

        Helpers.GenerateDummyFile(_input.Path, _content);

        var result = SFTP.ReadFile(_input, _connection);
        Assert.AreEqual(_content, result.Content);
        
        _input.FileEncoding = FileEncoding.ASCII;
        result = SFTP.ReadFile(_input, _connection);
        Assert.AreEqual(_content, result.Content);

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = true;
        result = SFTP.ReadFile(_input, _connection);
        Assert.AreEqual(_content, result.Content);

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = false;
        result = SFTP.ReadFile(_input, _connection);
        Assert.AreEqual(_content, result.Content);

        _input.FileEncoding = FileEncoding.WINDOWS1252;
        result = SFTP.ReadFile(_input, _connection);
        Assert.AreEqual(_content, result.Content);

        _input.FileEncoding = FileEncoding.Other;
        _input.EncodingInString = "iso-8859-1";
        result = SFTP.ReadFile(_input, _connection);
        Assert.AreEqual(_content, result.Content);
    }
}

