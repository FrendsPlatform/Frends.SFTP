using System;
using System.IO;
using System.Threading.Tasks;
using Frends.SFTP.RenameFile.Enums;
using NUnit.Framework;

namespace Frends.SFTP.RenameFile.Tests;

[TestFixture]
public class RenameTests : RenameFileTestBase
{
    [Test]
    public async Task RenameFile_SimpleTest()
    {
        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestRenameBehaviourRename()
    {
        await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);

        Helpers.GenerateDummyFile(TestInput.Path);
        TestInput.RenameBehaviour = RenameBehaviour.Rename;
        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestRenameBehaviourOverwrite()
    {
        await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);

        Helpers.GenerateDummyFile(TestInput.Path);
        TestInput.RenameBehaviour = RenameBehaviour.Overwrite;
        var result = await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Path));
    }

    [Test]
    public async Task RenameFile_TestRenameBehaviourThrow()
    {
        await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default);

        Helpers.GenerateDummyFile(TestInput.Path);
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await SFTP.RenameFile(TestInput, TestConnection, TestOptions, default));
        Assert.AreEqual($"File already exists {Path.Combine(Path.GetDirectoryName(TestInput.Path), TestInput.NewFileName).Replace("\\", "/")}. No file renamed.", ex.Message);
    }
}
