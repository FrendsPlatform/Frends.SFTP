using NUnit.Framework;
using System.IO;
using System.Threading;
using Frends.SFTP.UploadFiles.Definitions;


namespace Frends.SFTP.UploadFiles.Tests;

[TestFixture]
class TransferTests : UploadFilesTestBase
{

    [Test]
    public void UploadFiles_TestSimpleTransfer()
    {
        var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TestUploadWithFileMaskEverything()
    {
        var source = new Source
        {
            Directory = _workDir,
            FileName = "*",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing
        };

        var result = SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);
    }

    [Test] 
    public void UploadFiles_TestWithOperationLogDisabled()
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

        var result = SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.OperationsLog.Count);
    }

    [Test]
    public void UploadFiles_TestWithMultipleSubdirectoriesInDestination()
    {
        var destination = new Destination
        {
            Directory = "/upload/Upload/sub",
            Action = DestinationAction.Error,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TestOneErrorInTransferWithMultipleFiles()
    {
        var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);

        var destination = new Destination
        {
            Directory = "/upload/Upload",
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
            Directory = _workDir,
            FileName = "*.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing
        };

        result = SFTP.UploadFiles(source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsFalse(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
        Assert.AreEqual(1, result.FailedTransferCount);
    }

    [Test]
    public void UploadFiles_TestWithFileMaskWithFileAlreadyInDestination()
    {
        var source = new Source
        {
            Directory = _workDir,
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

        var result = SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        source.FileName = "*.txt";
        result = SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsFalse(result.Success);
        Assert.AreEqual(1, result.FailedTransferCount);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TestAppendToExistingFile()
    {
        var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var fullPath = _destination.Directory + "/" + _source.FileName;
        var content1 = Helpers.GetTransferredFileContent(fullPath);

        var destination = new Destination
        {
            Directory = "/upload/Upload",
            FileName = "SFTPUploadTestFile.txt",
            Action = DestinationAction.Append,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        var source = new Source
        {
            Directory = _workDir,
            FileName = "SFTPUploadTestFile2.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing
        };

        result = SFTP.UploadFiles(source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var content2 = Helpers.GetTransferredFileContent(fullPath);
        Assert.AreNotEqual(content1, content2);
    }

    [Test]
    public void UploadFiles_TestSourceOperationWithMove()
    {
        var to = Path.Combine(_workDir, "uploaded");
        Directory.CreateDirectory(to);
        var source = new Source
        {
            Directory = _workDir,
            FileName = "SFTPUploadTestFile.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = to
        };

        var result = SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(Path.Combine(to, source.FileName)));
        File.Move(Path.Combine(to, source.FileName), Path.Combine(_workDir, source.FileName));
        Directory.Delete(to);
    }

    [Test]
    public void UploadFiles_TestSourceOperationWithRename()
    {
        var source = new Source
        {
            Directory = _workDir,
            FileName = "SFTPUploadTestFile.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Rename,
            FileNameAfterTransfer = "uploaded_%SourceFileName%.txt"
        };

        var result = SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(Path.Combine(_workDir, "uploaded_SFTPUploadTestFile.txt")));
        File.Move(Path.Combine(_workDir, "uploaded_SFTPUploadTestFile.txt"), Path.Combine(_workDir, "SFTPUploadTestFile.txt"));
    }

    [Test]
    public void UploadFiles_TestSingleFileTransferWithError()
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

        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileName = "",
            Action = DestinationAction.Error,
        };

        Helpers.UploadSingleTestFile(_destination.Directory, Path.Combine(_workDir, "SFTPUploadTestFile.txt"));

        var result = SFTP.UploadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsFalse(result.Success);
        Assert.AreEqual(1, result.FailedTransferCount);
    }

    [Test]
    public void UploadFiles_SourceOperationNothingWithRenamingDisable()
    {
        var options = new Options
        {
            ThrowErrorOnFail = false,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };
        var result = SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);

        Assert.IsTrue(File.Exists(Path.Combine(_workDir, _source.FileName)));
    }

    [Test]
    public void UploadFiles_TestSourceOperationMoveWithRenameFilesDuringTransferDisabled()
    {
        var to = Path.Combine(_workDir, "destination");
        Directory.CreateDirectory(to);
        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = _workDir,
            FileName = "SFTPUploadTestFile.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = to
        };

        var result = SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(Path.Combine(to, source.FileName)));
        File.Move(Path.Combine(to, source.FileName), Path.Combine(_workDir, source.FileName));
        Directory.Delete(to, true);
    }

    [Test]
    public void UploadFiles_TestSourceOperationRenameWithRenameFilesDuringTransferDisabled()
    {
        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = _workDir,
            FileName = "SFTPUploadTestFile.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Rename,
            FileNameAfterTransfer = "uploaded_%SourceFileName%%SourceFileExtension%"
        };

        var result = SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(Path.Combine(_workDir, "uploaded_" + source.FileName)));
        File.Move(Path.Combine(_workDir, "uploaded_" + source.FileName), Path.Combine(_workDir, source.FileName));
    }

    [Test]
    public void UploadFiles_TestSourceOperationMoveWithRenameSourceAndDestinationFilesEnabled()
    {
        var to = Path.Combine(_workDir, "destination");
        Directory.CreateDirectory(to);

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
            Directory = _workDir,
            FileName = "SFTPUploadTestFile.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = to
        };

        var result = SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(Path.Combine(to, source.FileName)));
        File.Move(Path.Combine(to, source.FileName), Path.Combine(_workDir, source.FileName));
        Directory.Delete(to, true);
    }                                                                                             

    [Test]
    public void UploadFiles_TestSourceOperationRenameWithRenameFilesDuringTransferWithRenameSourceAndDestinationFilesEnabled()
    {
        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        var source = new Source
        {
            Directory = _workDir,
            FileName = "SFTPUploadTestFile.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Rename,
            FileNameAfterTransfer = "uploaded_%SourceFileName%%SourceFileExtension%"
        };

        var result = SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(File.Exists(Path.Combine(_workDir, "uploaded_" + source.FileName)));

        File.Move(Path.Combine(_workDir, "uploaded_SFTPUploadTestFile.txt"), Path.Combine(_workDir, "SFTPUploadTestFile.txt"));
    }

    [Test]
    public void UploadFiles_TestUploadWithOverwrite()
    {
        var destination = new Destination
        {
            Directory = "/upload/Upload",
            FileName = "SFTPUploadTestFile.txt",
            Action = DestinationAction.Overwrite,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        Helpers.UploadSingleTestFile(destination.Directory, Path.Combine(_workDir, _source.FileName));

        var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.CheckFileExistsInDestination(destination.Directory + "/" + _source.FileName));
    }

    [Test]
    public void UploadFiles_TestUploadWithOnlyRenameSourceDuringTransferEnabled()
    {
        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        var result = SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.CheckFileExistsInDestination(_destination.Directory + "/" + _source.FileName));
    }

    [Test]
    public void UploadFiles_TestUploadWithOnlyRenameDestinationDuringTransferEnabled()
    {
        var options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        var result = SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.CheckFileExistsInDestination(_destination.Directory + "/" + _source.FileName));
    }

    [Test]
    public void UploadFiles_TestWithSourceMoveToNonExistingDirectoryShouldReturnUnsuccessfulTransfer()
    {
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
            DirectoryToMoveAfterTransfer = Path.Combine(_workDir, "uploaded"),
        };

        var result = SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
        Assert.IsFalse(result.Success);

        Assert.IsTrue(File.Exists(Path.Combine(_workDir, _source.FileName)));
    }

    [Test]
    public void UploadFiles_NoSourceFilesAndIgnoreShouldNotThrowException()
    {
        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "NonExistingFile.txt",
            Action = SourceAction.Ignore,
            Operation = SourceOperation.Delete
        };

        var result = SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.ActionSkipped);
    }

    [Test]
    public void UploadFiles_NoSourceFilesAndInfoShouldNotThrowException()
    {
        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "NonExistingFile.txt",
            Action = SourceAction.Info,
            Operation = SourceOperation.Delete
        };

        var result = SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.ActionSkipped);
    }
}

