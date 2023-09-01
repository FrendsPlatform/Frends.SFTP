namespace Frends.SFTP.DeleteFiles.Tests;

using NUnit.Framework;
using Renci.SshNet.Common;

/// <summary>
/// Test class.
/// </summary>
[TestFixture]
internal class UnitTests : UnitTestBase
{
    [Test]
    public void DeleteFiles_SimpleDelete()
    {
        var result = SFTP.DeleteFiles(_input, _connection, default);
        Assert.AreEqual(3, result.Files.Count);
    }

    [Test]
    public void DeleteFiles_TaskShouldNotThrowIfNoFiles()
    {
        _input.FileMask = "FileThatDontExist";
        var result = SFTP.DeleteFiles(_input, _connection, default);
        Assert.AreEqual(0, result.Files.Count);
    }

    [Test]
    public void DeleteFiles_TestWithFilePaths()
    {
        var filePaths = new string[] { "/delete/subDir/test2.txt", "/delete/subDir/test1.txt" };
        _input.FilePaths = filePaths;
        var result = SFTP.DeleteFiles(_input, _connection, default);
        Assert.AreEqual(2, result.Files.Count);
    }

    [Test]
    public void DeleteFiles_TestWithDirectoryNotExisting()
    {
        _input.Directory = "/does/not/exist";
        var ex = Assert.Throws<SftpPathNotFoundException>(() => SFTP.DeleteFiles(_input, _connection, default));
        Assert.AreEqual($"No such Directory '{_input.Directory}'.", ex.Message);
    }
}
