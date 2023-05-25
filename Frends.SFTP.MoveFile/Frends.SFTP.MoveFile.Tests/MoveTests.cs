using NUnit.Framework;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile.Tests;

[TestFixture]
class MoveTests : MoveFileTestBase
{
    [Test]
    public void MoveFile_TestSimpleMove()
    {
        var result = SFTP.MoveFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }

    [Test]
    public void MoveFile_TestMoveOverwrite()
    {
        var result = SFTP.MoveFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
        _input.IfTargetFileExists = FileExistsOperation.Overwrite;

        Helpers.GenerateDummyFile("test.txt");
        result = SFTP.MoveFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }

    [Test]
    public void MoveFile_TestWithPatterns()
    {
        _input.Pattern = "*.txt";
        var result = SFTP.MoveFile(_input, _connection, default);
        Assert.AreEqual(1, result.Files.Count);

        Helpers.GenerateDummyFile("test.txt");
        _input.Pattern = "te*";
        _input.IfTargetFileExists = FileExistsOperation.Overwrite;
        result = SFTP.MoveFile(_input, _connection, default);
        Assert.AreEqual(1, result.Files.Count);
    }

    [Test]
    public void MoveFile_TestRename()
    {
        var result = SFTP.MoveFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
        _input.IfTargetFileExists = FileExistsOperation.Rename;

        Helpers.GenerateDummyFile("test.txt");
        result = SFTP.MoveFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }
}

