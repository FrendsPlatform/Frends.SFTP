using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.MoveFile.Enums;
using NUnit.Framework;

namespace Frends.SFTP.MoveFile.Tests;

[TestFixture]
internal class MoveTests : MoveFileTestBase
{
    [Test]
    public async Task MoveFile_TestSimpleMove()
    {
        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Error);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }

    [Test]
    public async Task MoveFile_Iso88591EncodingShouldWork()
    {
        TestInput.FileEncoding = FileEncoding.Other;
        TestInput.EncodingInString = "iso-8859-1";

        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }

    [Test]
    public void MoveFile_InvalidEncodingNameShouldThrowClearError()
    {
        TestInput.FileEncoding = FileEncoding.Other;
        TestInput.EncodingInString = "iso-invalid-encoding";

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default));
        Assert.IsTrue(ex.Message.Contains("Unknown encoding: 'iso-invalid-encoding'"), ex.Message);
    }

    [Test]
    public async Task MoveFile_TestMoveOverwrite()
    {
        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
        TestInput.IfTargetFileExists = FileExistsOperation.Overwrite;

        Helpers.GenerateDummyFile("test.txt");
        result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }

    [Test]
    public async Task MoveFile_TestWithPatterns()
    {
        TestInput.Pattern = "*.txt";
        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.AreEqual(1, result.Files.Count);

        Helpers.GenerateDummyFile("test.txt");
        TestInput.Pattern = "te*";
        TestInput.IfTargetFileExists = FileExistsOperation.Overwrite;
        result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.AreEqual(1, result.Files.Count);
    }

    [Test]
    public async Task MoveFile_TestRename()
    {
        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
        TestInput.IfTargetFileExists = FileExistsOperation.Rename;

        Helpers.GenerateDummyFile("test.txt");
        result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }

    [Test]
    public async Task MoveFile_CreatesNestedDirectories()
    {
        TestInput.CreateTargetDirectories = true;
        TestInput.TargetDirectory = $"/upload/{Guid.NewGuid()}/test/nested/directory";
        var result = await SFTP.MoveFile(TestInput, TestConnection, TestOptions, CancellationToken.None);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }
}
