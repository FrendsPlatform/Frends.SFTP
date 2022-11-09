using NUnit.Framework;
using System.Threading;
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
    public void ListFilesWithIncludeSubdirectoriesDisabled()
    {
        _input.FileMask = "*.txt";

        var result = SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public void ListFilesWithIncludeSubdirectoriesEnabled()
    {
        _input.FileMask = "*.txt";
        _input.IncludeSubdirectories = true;
            
        var result = SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(6, result.FileCount);
    }

    [Test]
    public void ListFilesWithoutFileMask()
    {
        var result = SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public void ListFilesWithIncludeTypeBoth()
    {
        _input.IncludeType = IncludeType.Both;

        var result = SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(4, result.FileCount);
    }

    [Test]
    public void ListFilesWithIncludeTypeDirectory()
    {
        _input.IncludeType = IncludeType.Directory;
        _input.IncludeSubdirectories = true;

        var result = SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(1, result.FileCount);
        Assert.IsTrue(result.Files[0].IsDirectory);
    }

    [Test]
    public void ListFiles_TestWithDifferentEncoding()
    {
        _input.IncludeType = IncludeType.Directory;
        _input.IncludeSubdirectories = true;

        var result = SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);

        _input.FileEncoding = FileEncoding.ASCII;
        result = SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = true;
        result = SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);

        _input.FileEncoding = FileEncoding.UTF8;
        _input.EnableBom = false;
        result = SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);

        _input.FileEncoding = FileEncoding.WINDOWS1252;
        result = SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);

        _input.FileEncoding = FileEncoding.Other;
        _input.EncodingInString = "iso-8859-1";
        result = SFTP.ListFiles(_input, _connection, new CancellationToken());
        Assert.AreEqual(1, result.FileCount);
    }
}

