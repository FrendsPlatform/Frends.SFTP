using NUnit.Framework;
using System;
using System.IO;
using Frends.SFTP.RenameFile.Enums;

namespace Frends.SFTP.RenameFile.Tests;

[TestFixture]
class RenameTests : RenameFileTestBase
{
    [Test]
    public void RenameFile_SimpleTest()
    {
        var result = SFTP.RenameFile(_input, _connection);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public void RenameFile_TestRenameBehaviourRename()
    {
        SFTP.RenameFile(_input, _connection);

        Helpers.GenerateDummyFile(_input.Path);
        _input.RenameBehaviour = RenameBehaviour.Rename;
        var result = SFTP.RenameFile(_input, _connection);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public void RenameFile_TestRenameBehaviourOverwrite()
    {
        SFTP.RenameFile(_input, _connection);

        Helpers.GenerateDummyFile(_input.Path);
        _input.RenameBehaviour = RenameBehaviour.Overwrite;
        var result = SFTP.RenameFile(_input, _connection);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public void RenameFile_TestRenameBehaviourThrow()
    {
        SFTP.RenameFile(_input, _connection);

        Helpers.GenerateDummyFile(_input.Path);
        var ex = Assert.Throws<ArgumentException>(() => SFTP.RenameFile(_input, _connection));
        Assert.AreEqual($"File already exists {Path.Combine(Path.GetDirectoryName(_input.Path), _input.NewFileName).Replace("\\", "/")}. No file renamed.", ex.Message);
    }
}

