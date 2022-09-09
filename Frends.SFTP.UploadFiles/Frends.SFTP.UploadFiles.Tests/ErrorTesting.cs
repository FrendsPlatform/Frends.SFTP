using NUnit.Framework;
using System.IO;
using System;
using System.Threading;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests;

[TestFixture]
class ErrorTesting : UploadFilesTestBase
{
    [Test]
    public void UploadFiles_TestTransferThatThrowsIfFileNotExist()
    {
        var source = new Source
        {
            Directory = _workDir,
            FileName = "FileThatDontExist.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("SFTP transfer failed:"));
    }

    [Test]
    public void UploadFiles_TestTransferThatExistsThrowsError()
    {
        var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);

        var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith($"SFTP transfer failed: 1 Errors: Failure in CheckIfDestination"));
    }

    [Test]
    public void UploadFiles_TestThrowsWithWrongPort()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Port = 51651;

        var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("SFTP transfer failed: Unable to establish the socket: No such host is known"));
    }

    [Test]
    public void UploadFiles_TestThrowsMovedSourceFileIsRestored()
    {
        Helpers.UploadSingleTestFile(_destination.Directory, Path.Combine(_workDir, _source.FileName));
        
        var connection = Helpers.GetSftpConnection();
        var source = new Source
        {
            Directory = _workDir,
            FileName = "SFTPUploadTestFile1.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = Path.Combine(_workDir, "moved")
        };
        Directory.CreateDirectory(source.DirectoryToMoveAfterTransfer);

        var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith($"SFTP transfer failed: 1 Errors: Failure in CheckIfDestination"));
        Assert.IsTrue(File.Exists(Path.Combine(_source.Directory, _source.FileName)));

        Directory.Delete(source.DirectoryToMoveAfterTransfer, true);
    }

    [Test]
    public void UploadFiles_TestSourceMoveWithFileAlreadyInMovedFolder()
    {
        Helpers.UploadSingleTestFile(_destination.Directory, Path.Combine(_workDir, _source.FileName));

        var connection = Helpers.GetSftpConnection();
        var source = new Source
        {
            Directory = _workDir,
            FileName = "SFTPUploadTestFile1.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = Path.Combine(_workDir, "moved")
        };
        Directory.CreateDirectory(source.DirectoryToMoveAfterTransfer);
        File.Copy(Path.Combine(source.Directory, source.FileName), Path.Combine(source.DirectoryToMoveAfterTransfer, source.FileName));

        var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.Contains($"Error: Failure in source operation:"));
        Assert.IsTrue(File.Exists(Path.Combine(_source.Directory, _source.FileName)));

        Directory.Delete(source.DirectoryToMoveAfterTransfer, true);
    }
}

