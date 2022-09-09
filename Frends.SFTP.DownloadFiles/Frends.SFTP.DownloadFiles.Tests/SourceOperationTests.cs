using NUnit.Framework;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests;

[TestFixture]
class SourceOperationTests : DownloadFilesTestBase
{
    [Test]
    public void DownloadFiles_SourceOperationNothingWithRenamingDisable()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var options = new Options
        {
            ThrowErrorOnFail = false,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };
        var result = SFTP.DownloadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);

        Assert.IsTrue(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
    }

    [Test]
    public void DownloadFiles_TestSourceOperationMove()
    {
        var to = "uploaded";
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory, to);
        to = "upload/Upload/" + to;
        var source = new Source
        {
            Directory = "upload/Upload",
            FileName = "SFTPDownloadTestFile1.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = to
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.SourceFileExists(source.DirectoryToMoveAfterTransfer + "/" + source.FileName));
    }

    [Test]
    public void DownloadFiles_TestWithSourceMoveToNonExistingDirectoryShouldReturnUnsuccessfulTransfer()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
        Directory.CreateDirectory(_destWorkDir);

        var options = new Options
        {
            ThrowErrorOnFail = false,
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
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = "/upload/test"
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsFalse(result.Success);
        Assert.IsTrue(Helpers.SourceFileExists(Path.Combine(_source.Directory, _source.FileName).Replace("\\", "/")));
    }

    [Test]
    public void DownloadFiles_TestSourceOperationRename()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var source = new Source
        {
            Directory = "upload/Upload",
            FileName = "SFTPDownloadTestFile1.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Rename,
            FileNameAfterTransfer = "uploaded_%SourceFileName%.txt"
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.SourceFileExists(source.Directory + "/uploaded_" + source.FileName));
    }

    [Test]
    public void DownloadFiles_TestSourceOperationMoveWithRenameFilesDuringTransfer()
    {
        var to = "uploaded";
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory, to);
        to = "upload/Upload/" + to;

        var options = new Options
        {
            ThrowErrorOnFail = false,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = "upload/Upload",
            FileName = "SFTPDownloadTestFile1.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = to
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.SourceFileExists(source.DirectoryToMoveAfterTransfer + "/" + source.FileName));
    }

    [Test]
    public void DownloadFiles_TestSourceOperationRenameWithRenameFilesDuringTransfer()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var options = new Options
        {
            ThrowErrorOnFail = false,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = "upload/Upload",
            FileName = "SFTPDownloadTestFile1.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Rename,
            FileNameAfterTransfer = "uploaded_%SourceFileName%.txt"
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.SourceFileExists(source.Directory + "/uploaded_" + source.FileName));
    }

    [Test]
    public void DownloadFiles_TestSourceOperationMoveWithRenameFilesDuringTransferWithRenameSourceAndDestinationFilesEnabled()
    {
        var to = "uploaded";
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory, to);
        to = "upload/Upload/" + to;

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
            Directory = "upload/Upload",
            FileName = "SFTPDownloadTestFile1.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = to
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.SourceFileExists(source.DirectoryToMoveAfterTransfer + "/" + source.FileName));
    }

    [Test]
    public void DownloadFiles_TestSourceOperationRenameWithRenameFilesDuringTransferWithRenameSourceAndDestinationFilesEnabled()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

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
            Directory = "upload/Upload",
            FileName = "SFTPDownloadTestFile1.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Rename,
            FileNameAfterTransfer = "uploaded_%SourceFileName%.txt"
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.SourceFileExists(source.Directory + "/uploaded_" + source.FileName));
    }

    [Test]
    public void DownloadFiles_SourceOperationDelete()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
        Directory.CreateDirectory(_destWorkDir);

        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = true,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = _source.FileName,
            Action = SourceAction.Error,
            Operation = SourceOperation.Delete
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsFalse(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
    }

    [Test]
    public void DownloadFiles_SourceOperationDeleteRenameSourceFile()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
        Directory.CreateDirectory(_destWorkDir);

        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = true,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = _source.FileName,
            Action = SourceAction.Error,
            Operation = SourceOperation.Delete
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsFalse(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
    }

    [Test]
    public void DownloadFiles_SourceOperationDeleteRenameDestinationFile()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
        Directory.CreateDirectory(_destWorkDir);

        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = true,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = _source.FileName,
            Action = SourceAction.Error,
            Operation = SourceOperation.Delete
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsFalse(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
    }

    [Test]
    public void DownloadFiles_SourceOperationDeleteRenameBoth()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
        Directory.CreateDirectory(_destWorkDir);

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
            Action = SourceAction.Error,
            Operation = SourceOperation.Delete
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsFalse(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
    }
}
