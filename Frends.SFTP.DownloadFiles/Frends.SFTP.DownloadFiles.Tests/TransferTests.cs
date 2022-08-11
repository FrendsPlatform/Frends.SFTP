using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests;

[TestFixture]
class TransferTests : DownloadFilesTestBase
{
    [Test]
    public void DownloadFiles_TestSimpleTransfer()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var result = SFTP.DownloadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestDownloadWithFileMask()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "*",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test] 
    public void DownloadFiles_TestWithOperationLogDisabled()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

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

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

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
        var files = new List<string>
        {
            Path.Combine(_workDir, "SFTPDownloadTestFile.txt"),
            Path.Combine(_workDir, "SFTPDownloadTestFile2.txt"),
            Path.Combine(_workDir, "SFTPDownloadTestFile3.txt")
        };
        Helpers.UploadTestFiles(files, _source.Directory);
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
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
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
            FileName = "*File.txt",
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

        var files = new List<string>
        {
            Path.Combine(_workDir, "SFTPDownloadTestFile.txt"),
            Path.Combine(_workDir, "SFTPDownloadTestFile2.txt"),
            Path.Combine(_workDir, "SFTPDownloadTestFile3.txt"),
        };
        Helpers.UploadTestFiles(files, _source.Directory);

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
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

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
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

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
            FileName = _source.FileName,
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
            FileName = _source.FileName,
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
}

