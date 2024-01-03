using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using Frends.SFTP.RenameFile.Enums;

namespace Frends.SFTP.RenameFile.Tests;

[TestFixture]
class RenameTests : RenameFileTestBase
{
    [Test]
    public async Task RenameFile_SimpleTest()
    {
        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestRenameBehaviourRename()
    {
        await SFTP.RenameFile(_input, _connection, default);

        Helpers.GenerateDummyFile(_input.Path);
        _input.RenameBehaviour = RenameBehaviour.Rename;
        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestRenameBehaviourOverwrite()
    {
        await SFTP.RenameFile(_input, _connection, default);

        Helpers.GenerateDummyFile(_input.Path);
        _input.RenameBehaviour = RenameBehaviour.Overwrite;
        var result = await SFTP.RenameFile(_input, _connection, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestRenameBehaviourThrow()
    {
        await SFTP.RenameFile(_input, _connection, default);

        Helpers.GenerateDummyFile(_input.Path);
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.RenameFile(_input, _connection, default));
        Assert.AreEqual($"File already exists {Path.Combine(Path.GetDirectoryName(_input.Path), _input.NewFileName).Replace("\\", "/")}. No file renamed.", ex.Message);
    }
}

