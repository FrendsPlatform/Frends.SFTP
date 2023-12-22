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
}

