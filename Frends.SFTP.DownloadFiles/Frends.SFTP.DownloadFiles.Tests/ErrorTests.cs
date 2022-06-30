using NUnit.Framework;
using System.IO;
using System;
using System.Threading;
using System.Collections.Generic;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests;

[TestFixture]
class ErrorTests : DownloadFilesTestBase
{
    [Test]
    public void DownloadFiles_TestTransferThatExistsThrowsError()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
        Directory.CreateDirectory(_destWorkDir);
        File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, _connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith($"SFTP transfer failed: 1 Errors: Failure in CheckIfDestination"));
    }

    [Test]
    public void DownloadFiles_TestTransferThatThrowsIfFileNotExist()
    {
        var source = new Source
        {
            Directory = "/upload",
            FileName = "FileThatDontExist.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("SFTP transfer failed: 1 Errors: No source files found from directory"));
    }

    [Test]
    public void DownloadFiles_TestThrowsWithWrongPort()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Port = 51651;

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("SFTP transfer failed: Unable to establish the socket: No such host is known"));
    }

    [Test]
    public void DownloadFiles_TestWithSubDirNameAsFileMask()
    {
        var path = "/upload/test";
        Helpers.CreateSubDirectory(path);
        var source = new Source
        {
            Directory = "/upload",
            FileName = "test",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("SFTP transfer failed: 1 Errors: No source files found from directory"));

        Helpers.DeleteSubDirectory(path);
    }
}
