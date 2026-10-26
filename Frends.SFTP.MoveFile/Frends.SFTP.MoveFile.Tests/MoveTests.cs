using System;
using System.Threading;
using NUnit.Framework;
using System.Threading.Tasks;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile.Tests;

[TestFixture]
class MoveTests : MoveFileTestBase
{
    [Test]
    public async Task MoveFile_TestSimpleMove()
    {
        var result = await SFTP.MoveFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }

    [Test]
    public async Task MoveFile_Iso88591EncodingShouldWork()
    {
        _input.FileEncoding = FileEncoding.Other;
        _input.EncodingInString = "iso-8859-1";

        var result = await SFTP.MoveFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }

    [Test]
    public void MoveFile_InvalidEncodingNameShouldThrowClearError()
    {
        _input.FileEncoding = FileEncoding.Other;
        _input.EncodingInString = "iso-invalid-encoding";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.MoveFile(_input, _connection, default));
        Assert.IsTrue(ex.Message.Contains("Unknown encoding: 'iso-invalid-encoding'"), ex.Message);
    }

    [Test]
    public async Task MoveFile_TestMoveOverwrite()
    {
        var result = await SFTP.MoveFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
        _input.IfTargetFileExists = FileExistsOperation.Overwrite;

        Helpers.GenerateDummyFile("test.txt");
        result = await SFTP.MoveFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }

    [Test]
    public async Task MoveFile_TestWithPatterns()
    {
        _input.Pattern = "*.txt";
        var result = await SFTP.MoveFile(_input, _connection, default);
        Assert.AreEqual(1, result.Files.Count);

        Helpers.GenerateDummyFile("test.txt");
        _input.Pattern = "te*";
        _input.IfTargetFileExists = FileExistsOperation.Overwrite;
        result = await SFTP.MoveFile(_input, _connection, default);
        Assert.AreEqual(1, result.Files.Count);
    }

    [Test]
    public async Task MoveFile_TestRename()
    {
        var result = await SFTP.MoveFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
        _input.IfTargetFileExists = FileExistsOperation.Rename;

        Helpers.GenerateDummyFile("test.txt");
        result = await SFTP.MoveFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }

    [Test]
    public async Task MoveFile_CreatesNestedDirectories()
    {
        _input.CreateTargetDirectories = true;
        _input.TargetDirectory = $"/upload/{Guid.NewGuid()}/test/nested/directory";
        var result = await SFTP.MoveFile(_input, _connection, CancellationToken.None);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }
}