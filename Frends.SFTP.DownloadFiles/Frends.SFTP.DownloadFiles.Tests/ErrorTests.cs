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

    [Test]
    public void DownloadFiles_TestThrowsWithSourceMoveToNonExistingDirectoryShouldReturnUnsuccessfulTransfer()
    {
        Directory.CreateDirectory(_destWorkDir);

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = _source.FileName,
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = "/upload/test"
        };

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.Contains($"Operation failed: Source file {_source.FileName} couldn't be moved to given directory {source.DirectoryToMoveAfterTransfer} because the directory didn't exist."));
        Assert.IsTrue(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
    }

    [Test]
    public void DownloadFiles_TestThrowsSourceMoveToDestinationFileExists()
    {
        Directory.CreateDirectory(_destWorkDir);
        Helpers.CreateSubDirectory("/upload/uploaded");
        File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

        var options = new Options
        {
            ThrowErrorOnFail = false,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = _source.FileName,
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = "/upload/uploaded/"
        };

        var destination = new Destination
        {
            Directory = _destWorkDir,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true,
            Action = DestinationAction.Error,
        };

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(source, destination, _connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith($"SFTP transfer failed: 1 Errors: Failure in CheckIfDestination"));
        Assert.IsTrue(Helpers.SourceFileExists(Path.Combine(_source.Directory, _source.FileName).Replace("\\", "/")));
    }

    [Test]
    public void DownloadFiles_TestThrowsSourceMoveToDestinationFileExistsWithRenameSourceFileBeforeTransfer()
    {
        Directory.CreateDirectory(_destWorkDir);
        Helpers.CreateSubDirectory("/upload/uploaded");
        File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = _source.FileName,
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = "/upload/uploaded/"
        };

        var destination = new Destination
        {
            Directory = _destWorkDir,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true,
            Action = DestinationAction.Error,
        };

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(source, destination, _connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith($"SFTP transfer failed: 1 Errors: Failure in CheckIfDestination"));
        Assert.IsTrue(Helpers.SourceFileExists(Path.Combine(_source.Directory, _source.FileName).Replace("\\", "/")));
    }

    [Test]
    public void DownloadFiles_TestErrorMessage()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Password = "cuinbeu8i9ch";

        var ex = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.Contains($"FRENDS SFTP file transfer '' from 'SFTP://localhost//upload/Upload/{_source.FileName}' to 'FILE://{_destination.Directory}':"));
    }
}
