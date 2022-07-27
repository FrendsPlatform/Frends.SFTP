using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests;

[TestFixture]
class PreserveModifiedTests : UploadFilesTestBase
{

    private Destination destination;
    private Options options;
    private string sourcePath;
    private string destFilePath;
    private DateTime date;

    [SetUp]
    public void SetUp()
    {
        destination = new Destination
        {
            Directory = "/upload/Upload/destination",
            Action = DestinationAction.Overwrite,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = false,
            RenameDestinationFileDuringTransfer = false,
            CreateDestinationDirectories = true,
            PreserveLastModified = true,
            OperationLog = true
        };

        sourcePath = Path.Combine(_workDir, _source.FileName);
        destFilePath = destination.Directory + "/" + _source.FileName;
        date = File.GetLastWriteTime(sourcePath);
    }

    [Test]
    public void UploadFiles_TestPreserveLastModifiedWithoutRename()
    {
        var result = SFTP.UploadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.CheckFileExistsInDestination(destFilePath));
        Assert.AreEqual(date.ToString(), Helpers.GetLastWriteTimeFromDestination(destFilePath));
    }

    [Test]
    public void UploadFiles_TestPreserveLastModifiedWithRename()
    {
        options.RenameSourceFileBeforeTransfer = true;
        options.RenameDestinationFileDuringTransfer = true;

        var result = SFTP.UploadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.CheckFileExistsInDestination(destFilePath));
        Assert.AreEqual(date.ToString(), Helpers.GetLastWriteTimeFromDestination(destFilePath));
    }

    [Test]
    public void UploadFiles_TestPreserveLastModifiedWithSourceRename()
    {
        options.RenameSourceFileBeforeTransfer = true;
        options.RenameDestinationFileDuringTransfer = false;

        var result = SFTP.UploadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.CheckFileExistsInDestination(destFilePath));
        Assert.AreEqual(date.ToString(), Helpers.GetLastWriteTimeFromDestination(destFilePath));
    }

    [Test]
    public void UploadFiles_TestPreserveLastModifiedWithDestinationRename()
    {
        options.RenameSourceFileBeforeTransfer = false;
        options.RenameDestinationFileDuringTransfer = true;

        var result = SFTP.UploadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.CheckFileExistsInDestination(destFilePath));
        Assert.AreEqual(date.ToString(), Helpers.GetLastWriteTimeFromDestination(destFilePath));
    }

    [Test]
    public void UploadFiles_TestPreserveLastModifiedWithRenameAndDeleteSourceFile()
    {
        options.RenameSourceFileBeforeTransfer = true;
        options.RenameDestinationFileDuringTransfer = true;

        var source = new Source
        {
            Directory = _workDir,
            FileName = "SFTPUploadTestFile(Copy).txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Delete,
        };

        File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_workDir, source.FileName), true);

        var result = SFTP.UploadFiles(source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        var destFilePath = destination.Directory + "/" + source.FileName;

        Assert.IsTrue(Helpers.CheckFileExistsInDestination(destFilePath));
        Assert.AreEqual(date.ToString(), Helpers.GetLastWriteTimeFromDestination(destFilePath));
    }
}

