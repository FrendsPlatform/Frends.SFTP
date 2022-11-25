using NUnit.Framework;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests;

[TestFixture]
class TransferTests : DownloadFilesTestBase
{
    [Test]
    public void DownloadFiles_TestSimpleTransfer()
    {
        var result = SFTP.DownloadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestDownloadWithFileMask()
    {
        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "*",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);
    }

    [Test] 
    public void DownloadFiles_TestWithOperationLogDisabled()
    {
        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = false
        };

        var result = SFTP.DownloadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.OperationsLog.Count);
    }

    [Test]
    public void DownloadFiles_TestWithMultipleSubdirectoriesInDestination()
    {
        var destination = new Destination
        {
            Directory = Path.Combine(_destWorkDir, "another\\folder"),
            Action = DestinationAction.Error,
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
        Assert.That(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
    }

    [Test]
    public void DownloadFiles_TestOneErrorInTransferWithMultipleFiles()
    {
        Directory.CreateDirectory(_destWorkDir);
        File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

        var destination = new Destination
        {
            Directory = _destWorkDir,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true,
            Action = DestinationAction.Error,
        };

        var options = new Options
        {
            ThrowErrorOnFail = false,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "*.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing
        };

        var result = SFTP.DownloadFiles(source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsFalse(result.Success);
        Assert.AreEqual(2, result.SuccessfulTransferCount);
        Assert.AreEqual(1, result.FailedTransferCount);
    }

    [Test]
    public void DownloadFiles_TestSingleFileTransferWithError()
    {
        var options = new Options
        {
            ThrowErrorOnFail = false,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        Directory.CreateDirectory(_destWorkDir);
        File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));

        var result = SFTP.DownloadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsFalse(result.Success);
        Assert.That(result.FailedTransferCount == 1);
    }

    [Test]
    public void DownloadFiles_TestWithFileMaskWithFileAlreadyInDestination()
    {
        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "*File1.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing
        };

        var options = new Options
        {
            ThrowErrorOnFail = false,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        source.FileName = "*.txt";
        result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsFalse(result.Success);
        Assert.AreEqual(1, result.FailedTransferCount);
        Assert.AreEqual(2, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestDownloadWithOverwrite()
    {
        var destination = new Destination
        {
            Directory = Path.Combine(_workDir, "destination"),
            Action = DestinationAction.Overwrite,
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
    }

    [Test]
    public void DownloadFiles_TestTransferWithRenameSourceEnabledRenameDestinationDisabled()
    {
        var destination = new Destination
        {
            Directory = Path.Combine(_workDir, "destination"),
            Action = DestinationAction.Overwrite,
        };

        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
    }

    [Test]
    public void DownloadFiles_NoSourceFilesAndIgnoreShouldNotThrowException()
    {
        Helpers.CreateSubDirectory("/upload/Upload");
        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = true,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "*.csv",
            Action = SourceAction.Ignore,
            Operation = SourceOperation.Delete
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.ActionSkipped);
    }

    [Test]
    public void DownloadFiles_NoSourceFilesAndInfoShouldNotThrowException()
    {
        Helpers.CreateSubDirectory("/upload/Upload");
        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = true,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "*.csv",
            Action = SourceAction.Info,
            Operation = SourceOperation.Delete
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.ActionSkipped);
    }

    [Test]
    public void DownloadFiles_TestNoSourceShouldNotCreateOperationsLogWhenSourceActionIsIgnore()
    {
        Directory.CreateDirectory(_destWorkDir);

        var source = new Source
        {
            Directory = "/upload/",
            FileName = _source.FileName,
            Action = SourceAction.Ignore,
            Operation = SourceOperation.Nothing,
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.OperationsLog.Count);
    }

    [Test]
    public void DownloadFiles_TestWithFilePaths()
    {
        var filePaths = Helpers.UploadTestFiles(_source.Directory, 3);

        var source = new Source
        {
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
            FilePaths = filePaths
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.AreEqual(3, result.SuccessfulTransferCount);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void DownloadFiles_TestWitFilePathsEvenIfSourceFileIsAssigned()
    {
        var filePaths = Helpers.UploadTestFiles(_source.Directory, 3);

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = _source.FileName,
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
            FilePaths = filePaths
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.AreEqual(3, result.SuccessfulTransferCount);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void DownloadFiles_TestTransferWithSpecialCharactersInFileNames()
    {
        // upload test files
        var files = new List<string> { "this is a test file.txt", "This_is(a test file).txt", "this is  { a test} file.txt" };
        Helpers.UploadTestFiles(_source.Directory, 0, null, files);

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "this is a test file.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        source = new Source
        {
            Directory = _source.Directory,
            FileName = "This_is(a test file).txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        source = new Source
        {
            Directory = _source.Directory,
            FileName = "this is  { a test} file.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }
}

