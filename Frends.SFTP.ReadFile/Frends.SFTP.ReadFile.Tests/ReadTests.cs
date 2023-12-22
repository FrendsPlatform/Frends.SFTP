using System.Threading.Tasks;
using NUnit.Framework;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile.Tests;

[TestFixture]
class ReadTests : ReadFileTestBase
{
    [Test]
    public async Task ReadFile_TestSimpleRead()
    {
        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public async Task ReadFile_TestWithEmptyContent()
    {
        var content = "";

        Helpers.OverrideDummyFile(_input.Path, content);

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(string.Empty, result.Content);
    }

    [Test]
    public async Task ReadFile_TestWithDifferentEncoding()
    {
        _input.FileEncoding = FileEncoding.ANSI;

        Helpers.GenerateDummyFile(_input.Path, _content);

        var result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);

        _input.FileEncoding = FileEncoding.ASCII;
        result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = true;
        result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = false;
        result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);

        _input.FileEncoding = FileEncoding.WINDOWS1252;
        result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);

        _input.FileEncoding = FileEncoding.Other;
        _input.EncodingInString = "iso-8859-1";
        result = await SFTP.ReadFile(_input, _connection, default);
        Assert.AreEqual(_content, result.Content);
    }
}

