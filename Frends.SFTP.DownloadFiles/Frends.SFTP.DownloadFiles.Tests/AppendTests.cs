using NUnit.Framework;
using System.IO;
using System;
using System.Threading;
using System.Collections.Generic;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests;

[TestFixture]
class AppendTests : DownloadFilesTestBase
{
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
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true,
            Action = DestinationAction.Append,
            FileContentEncoding = FileEncoding.UTF8,
            EnableBomForContent = true
        };

        var source = new Source
        {
            Directory = "/upload/Upload",
            FileName = "SFTPDownloadTestFile2.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        var result = SFTP.DownloadFiles(source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var file2 = new FileInfo(Path.Combine(_destWorkDir, _source.FileName));
        Assert.AreNotEqual(file1.Length, file2.Length);
    }

    [Test]
    public void DownloadFiles_AppendingToExistingFile()
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

        var destination = new Destination
        {
            Directory = _destWorkDir,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true,
            Action = DestinationAction.Append,
            FileContentEncoding = FileEncoding.UTF8,
            EnableBomForContent = true
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var content1 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));

        result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var content2 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
        Assert.IsTrue(content2.Length > content1.Length);
    }

    [Test]
    public void DownloadFiles_AppendingToExistingFileRenameSourceFile()
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

        var destination = new Destination
        {
            Directory = _destWorkDir,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true,
            Action = DestinationAction.Append,
            FileContentEncoding = FileEncoding.UTF8,
            EnableBomForContent = true
        };
        var result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var content1 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));

        result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var content2 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
        Assert.IsTrue(content2.Length > content1.Length);
    }

    [Test]
    public void DownloadFiles_AppendingToExistingFileRenameDestinationFile()
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

        var destination = new Destination
        {
            Directory = _destWorkDir,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true,
            Action = DestinationAction.Append,
            FileContentEncoding = FileEncoding.UTF8,
            EnableBomForContent = true
        };
        var result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var content1 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));

        result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var content2 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
        Assert.IsTrue(content2.Length > content1.Length);
    }

    [Test]
    public void DownloadFiles_AppendingToExistingFileRenameBoth()
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

        var destination = new Destination
        {
            Directory = _destWorkDir,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true,
            Action = DestinationAction.Append,
            FileContentEncoding = FileEncoding.UTF8,
            EnableBomForContent = true
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var content1 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));

        result = SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var content2 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
        Assert.IsTrue(content2.Length > content1.Length);
    }

    [Test]
    public void DownloadFiles_AppendingToExistingFileRenameBothWithSourceFileNameStar()
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

        var destination = new Destination
        {
            Directory = _destWorkDir,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true,
            Action = DestinationAction.Append,
            FileContentEncoding = FileEncoding.UTF8,
            EnableBomForContent = true
        };

        var source = new Source
        {
            Directory = "/upload/Upload",
            FileName = "*",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        var result = SFTP.DownloadFiles(source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var content1 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));

        result = SFTP.DownloadFiles(source, destination, _connection, options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var content2 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
        Assert.IsTrue(content2.Length > content1.Length);
    }
}
