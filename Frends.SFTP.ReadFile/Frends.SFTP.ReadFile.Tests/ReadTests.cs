using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.ReadFile.Definitions;
using NUnit.Framework;
using Frends.SFTP.ReadFile.Enums;
using Frends.SFTP.ReadFile.Tests.Lib;

namespace Frends.SFTP.ReadFile.Tests;

[TestFixture]
internal class ReadTests : ReadFileTestBase
{
    [Test]
    public async Task ReadFile_TestSimpleRead()
    {
        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);
        Assert.AreEqual(Array.Empty<byte>(), result.BinaryContent);
    }

    [Test]
    public async Task ReadFile_TestReadBinary()
    {
        Options.ContentType = ContentType.Binary;
        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.That(result.BinaryContent.Length, Is.GreaterThan(0));
        Assert.AreEqual(string.Empty, result.TextContent);
    }

    [Test]
    public async Task ReadFile_TestWithEmptyContent()
    {
        var content = "";

        Helpers.OverrideDummyFile(Input.Path, content);

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(string.Empty, result.TextContent);
    }

    [Test]
    public async Task ReadFile_TestWithDifferentEncoding()
    {
        Input.FileEncoding = FileEncoding.ANSI;

        Helpers.GenerateDummyFile(Input.Path, Content);

        var result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);

        Input.FileEncoding = FileEncoding.ASCII;
        result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);

        Input.FileEncoding = FileEncoding.UTF8;
        Input.EnableBom = true;
        result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);

        Input.FileEncoding = FileEncoding.UTF8;
        Input.EnableBom = false;
        result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);

        Input.FileEncoding = FileEncoding.WINDOWS1252;
        result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);

        Input.FileEncoding = FileEncoding.Other;
        Input.EncodingInString = "iso-8859-1";
        result = await SFTP.ReadFile(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(Content, result.TextContent);
    }
}
