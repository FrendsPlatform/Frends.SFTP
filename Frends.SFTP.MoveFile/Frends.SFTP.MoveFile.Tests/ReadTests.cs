using NUnit.Framework;
using System;
using System.IO;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile.Tests;

[TestFixture]
class ReadTests : MoveFileTestBase
{
    [Test]
    public void MoveFile_TestSimpleMove()
    {
        Helpers.GenerateDummyFile(Path.Combine(_input.Directory, "test.txt").Replace("\\", "/"));
        var result = SFTP.MoveFile(_input, _connection, default);
        var test = result.Files;
        var test1 = String.Join(",", result.Files);
        Assert.IsTrue(Helpers.DestinationFileExists(result.Files[0].DestinationPath));
    }
}

