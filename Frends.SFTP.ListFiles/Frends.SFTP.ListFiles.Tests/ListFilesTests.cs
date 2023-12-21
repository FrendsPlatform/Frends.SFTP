using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.ListFiles.Enums;

namespace Frends.SFTP.ListFiles.Tests;

/// <summary>
/// NOTE: To run these unit tests, you need an SFTP test server.
/// 
/// docker-compose up -d
/// 
/// </summary>
[TestFixture]
public class ListFilesTest : ListFilesTestBase
{
    [Test]
    public async Task ListFilesWithIncludeSubdirectoriesDisabled()
    {
        _input.FileMask = "*.txt";

        var result = await SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public async Task ListFilesWithIncludeSubdirectoriesEnabled()
    {
        _input.FileMask = "*.txt";
        _input.IncludeSubdirectories = true;

        var result = await SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(6, result.FileCount);
    }

    [Test]
    public async Task ListFilesWithoutFileMask()
    {
        var result = await SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public async Task ListFilesWithIncludeTypeBoth()
    {
        _input.IncludeType = IncludeType.Both;

        var result = await SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(4, result.FileCount);
    }

    [Test]
    public async Task ListFilesWithIncludeTypeDirectory()
    {
        _input.IncludeType = IncludeType.Directory;
        _input.IncludeSubdirectories = true;

        var result = await SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(1, result.FileCount);
        Assert.IsTrue(result.Files[0].IsDirectory);
    }

    [Test]
    public async Task ListFiles_TestWithDifferentEncoding()
    {
        _input.IncludeType = IncludeType.Directory;
        _input.IncludeSubdirectories = true;

        var result = await SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);

        _input.FileEncoding = FileEncoding.ASCII;
        result = await SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = true;
        result = await SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = false;
        result = await SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);

        _input.FileEncoding = FileEncoding.WINDOWS1252;
        result = await SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);

        _input.FileEncoding = FileEncoding.Other;
        _input.EncodingInString = "iso-8859-1";
        result = await SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);
    }
}

