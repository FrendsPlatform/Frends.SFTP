using NUnit.Framework;
using System.Threading;
using Frends.SFTP.ListFiles.Definitions;
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

        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Directory = "/listfiles",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI
        };
        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));
    }

    [Test]
    public void ListFilesWithIncludeSubdirectoriesEnabled()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Directory = "/listfiles/",
            FileMask = "*.txt",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI
        };
        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(6));
    }

    [Test]
    public void ListFilesWithoutFileMask()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Directory = "/listfiles/",
            FileMask = "",
            IncludeType = IncludeType.File,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI
        };
        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));
    }

    [Test]
    public void ListFilesWithIncludeTypeBoth()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Directory = "/listfiles/",
            FileMask = "",
            IncludeType = IncludeType.Both,
            IncludeSubdirectories = false,
            FileEncoding = FileEncoding.ANSI
        };
        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(4));
    }

    [Test]
    public void ListFilesWithIncludeTypeDirectory()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Directory = "/listfiles/",
            FileMask = "",
            IncludeType = IncludeType.Directory,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI
        };
        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.IsTrue(result[0].IsDirectory);
    }

    [Test]
    public void ListFiles_TestWithDifferentEncoding()
    {
        var connection = Helpers.GetSftpConnection();
        var input = new Input
        {
            Directory = "/listfiles/",
            FileMask = "",
            IncludeType = IncludeType.Directory,
            IncludeSubdirectories = true,
            FileEncoding = FileEncoding.ANSI
        };

        var result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(1, result.Count);

        input.FileEncoding = FileEncoding.ASCII;
        result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(1, result.Count);

        input.FileEncoding = FileEncoding.UTF8;
        input.EnableBom = true;
        result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(1, result.Count);

        input.FileEncoding = FileEncoding.UTF8;
        input.EnableBom = false;
        result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(1, result.Count);

        input.FileEncoding = FileEncoding.WINDOWS1252;
        result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(1, result.Count);

        input.FileEncoding = FileEncoding.Other;
        input.EncodingInString = "utf-8";
        result = SFTP.ListFiles(input, connection, new CancellationToken());
        Assert.AreEqual(1, result.Count);
    }
}

