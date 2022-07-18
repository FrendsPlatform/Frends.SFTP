using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
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
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
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
            Action = DestinationAction.Error,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
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
    public void DownloadFiles_TestAppendToExistingFile()
    {
        Directory.CreateDirectory(_destWorkDir);
        File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));
        var file1 = new FileInfo(Path.Combine(_workDir, _source.FileName));
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, "SFTPDownloadTestFile2.txt") }, _source.Directory);

        var destination = new Destination
        {
            Directory = _destWorkDir,
            FileName = _source.FileName,
            Action = DestinationAction.Append,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        var source = new Source
        {
            Directory = "/upload/Upload",
            FileName = "SFTPDownloadTestFile2.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing
        };

        var result = SFTP.DownloadFiles(source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var file2 = new FileInfo(Path.Combine(_destWorkDir, _source.FileName));
        Assert.AreNotEqual(file1.Length, file2.Length);
    }

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
            FileName = "SFTPDownloadTestFile.txt",
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
    public void DownloadFiles_TestSourceOperationRename()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var source = new Source
        {
            Directory = "upload/Upload",
            FileName = "SFTPDownloadTestFile.txt",
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
            FileName = "SFTPDownloadTestFile.txt",
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
            FileName = "SFTPDownloadTestFile.txt",
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
            FileName = "SFTPDownloadTestFile.txt",
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
            FileName = "SFTPDownloadTestFile.txt",
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
    public void DownloadFiles_TestDownloadWithOverwrite()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var destination = new Destination
        {
            Directory = Path.Combine(_workDir, "destination"),
            Action = DestinationAction.Overwrite,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
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
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
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
    public void DownloadFiles_TestPreserveLastModifiedWithoutRename()
    {
        var sourcePath = Path.Combine(_workDir, _source.FileName);
        Helpers.UploadTestFiles(new List<string> { sourcePath }, _source.Directory);
        var date = File.GetLastWriteTime(sourcePath);
        Helpers.SetTestFileLastModified(_source.Directory + "/" + _source.FileName, date);
        var destination = new Destination
        {
            Directory = Path.Combine(_workDir, "destination"),
            Action = DestinationAction.Overwrite,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = true,
            OperationLog = true
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        var destFilePath = Path.Combine(destination.Directory, _source.FileName);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(destFilePath));
        Assert.AreEqual(date.ToString(), File.GetLastWriteTime(destFilePath).ToString());
    }

    [Test]
    public void DownloadFiles_TestPreserveLastModifiedWithRename()
    {
        var sourcePath = Path.Combine(_workDir, _source.FileName);
        Helpers.UploadTestFiles(new List<string> { sourcePath }, _source.Directory);
        var date = File.GetLastWriteTime(sourcePath);
        Helpers.SetTestFileLastModified(_source.Directory + "/" + _source.FileName, date);
        var destination = new Destination
        {
            Directory = Path.Combine(_workDir, "destination"),
            Action = DestinationAction.Overwrite,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = true,
            OperationLog = true
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        var destFilePath = Path.Combine(destination.Directory, _source.FileName);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(destFilePath));
        Assert.AreEqual(date.ToString(), File.GetLastWriteTime(destFilePath).ToString());
    }

    [Test]
    public void DownloadFiles_TestPreserveLastModifiedWithSourceRename()
    {
        var sourcePath = Path.Combine(_workDir, _source.FileName);
        Helpers.UploadTestFiles(new List<string> { sourcePath }, _source.Directory);
        var date = File.GetLastWriteTime(sourcePath);
        Helpers.SetTestFileLastModified(_source.Directory + "/" + _source.FileName, date);
        var destination = new Destination
        {
            Directory = Path.Combine(_workDir, "destination"),
            Action = DestinationAction.Overwrite,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = true,
            OperationLog = true
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        var destFilePath = Path.Combine(destination.Directory, _source.FileName);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(destFilePath));
        Assert.AreEqual(date.ToString(), File.GetLastWriteTime(destFilePath).ToString());
    }

    [Test]
    public void DownloadFiles_TestPreserveLastModifiedWithDestinationRename()
    {
        var sourcePath = Path.Combine(_workDir, _source.FileName);
        Helpers.UploadTestFiles(new List<string> { sourcePath }, _source.Directory);
        var date = File.GetLastWriteTime(sourcePath);
        Helpers.SetTestFileLastModified(_source.Directory + "/" + _source.FileName, date);
        var destination = new Destination
        {
            Directory = Path.Combine(_workDir, "destination"),
            Action = DestinationAction.Overwrite,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = true,
            OperationLog = true
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        var destFilePath = Path.Combine(destination.Directory, _source.FileName);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(destFilePath));
        Assert.AreEqual(date.ToString(), File.GetLastWriteTime(destFilePath).ToString());
    }

    [Test]
    public void DownloadFiles_TestPreserveLastModifiedWithRenameAndDeleteSourceFile()
    {
        var sourcePath = Path.Combine(_workDir, _source.FileName);
        Helpers.UploadTestFiles(new List<string> { sourcePath }, _source.Directory);
        var date = File.GetLastWriteTime(sourcePath);
        Helpers.SetTestFileLastModified(_source.Directory + "/" + _source.FileName, date);
        var destination = new Destination
        {
            Directory = Path.Combine(_workDir, "destination"),
            Action = DestinationAction.Overwrite,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

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
            Directory = "upload/Upload",
            FileName = "SFTPDownloadTestFile.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Delete,
        };

        var result = SFTP.DownloadFiles(source, destination, _connection, options, _info, new CancellationToken());
        var destFilePath = Path.Combine(destination.Directory, _source.FileName);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(destFilePath));
        Assert.AreEqual(date.ToString(), File.GetLastWriteTime(destFilePath).ToString());
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
        Assert.IsTrue(Helpers.SourceFileExists(_source.Directory + "/" + _source.FileName));
    }
}

