using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.ListFiles.Enums;
using NUnit.Framework;

namespace Frends.SFTP.ListFiles.Tests;

/// <summary>
/// NOTE: To run these unit tests, you need an SFTP test server.
///
/// docker-compose up -d.
///
/// </summary>
[TestFixture]
public class ListFilesTests : ListFilesTestBase
{
    [Test]
    public async Task ListFilesWithIncludeSubdirectoriesDisabled()
    {
        Input.FileMask = "*.txt";

        var result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public async Task ListFilesWithIncludeSubdirectoriesEnabled()
    {
        Input.FileMask = "*.txt";
        Input.IncludeSubdirectories = true;

        var result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.IsNotNull(result);
        Assert.AreEqual(6, result.FileCount);
    }

    [Test]
    public async Task ListFilesWithoutFileMask()
    {
        var result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public async Task ListFilesWithIncludeTypeBoth()
    {
        Input.IncludeType = IncludeType.Both;

        var result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.FileCount);
    }

    [Test]
    public async Task ListFilesWithIncludeTypeDirectory()
    {
        Input.IncludeType = IncludeType.Directory;
        Input.IncludeSubdirectories = true;

        var result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.FileCount);
        Assert.IsTrue(result.Files[0].IsDirectory);
    }

    [Test]
    public async Task ListFiles_Iso88591EncodingShouldWork()
    {
        Input.FileEncoding = FileEncoding.Other;
        Input.EncodingInString = "iso-8859-1";

        var result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(3, result.FileCount);
    }

    [Test]
    public void ListFiles_InvalidEncodingNameShouldThrowClearError()
    {
        Input.FileEncoding = FileEncoding.Other;
        Input.EncodingInString = "iso-invalid-encoding";

        var ex = Assert.ThrowsAsync<System.ArgumentException>(async () => await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("Unknown encoding: 'iso-invalid-encoding'"), ex.Message);
    }

    [Test]
    public async Task ListFiles_TestWithDifferentEncoding()
    {
        Input.IncludeType = IncludeType.Directory;
        Input.IncludeSubdirectories = true;

        var result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(1, result.FileCount);

        Input.FileEncoding = FileEncoding.ASCII;
        result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(1, result.FileCount);

        Input.FileEncoding = FileEncoding.UTF8;
        Input.EnableBom = true;
        result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(1, result.FileCount);

        Input.FileEncoding = FileEncoding.UTF8;
        Input.EnableBom = false;
        result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(1, result.FileCount);

        Input.FileEncoding = FileEncoding.WINDOWS1252;
        result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(1, result.FileCount);

        Input.FileEncoding = FileEncoding.Other;
        Input.EncodingInString = "iso-8859-1";
        result = await SFTP.ListFiles(Input, Connection, Options, CancellationToken.None);
        Assert.AreEqual(1, result.FileCount);
    }
}
