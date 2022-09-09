using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests;

[TestFixture]
class EncodingTests : DownloadFilesTestBase
{
    [SetUp]
    public void setup()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
    }

    [Test]
    public void UploadFiles_TransferWithANSIFileNameEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileNameEncoding = FileEncoding.ANSI
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TransferWithASCIIFileNameEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileNameEncoding = FileEncoding.ASCII
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TransferWithUTF8WithoutBomFileNameEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = false
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TransferWithUTF8WithBomFileNameEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TransferWithWin1252FileNameEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileNameEncoding = FileEncoding.WINDOWS1252
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TransferWithOtherFileNameEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileNameEncoding = FileEncoding.Other,
            FileNameEncodingInString = "windows-1252"
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TransferWithASCIIFileContentEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileContentEncoding = FileEncoding.ASCII,
            Action = DestinationAction.Append
        };

        Directory.CreateDirectory(destination.Directory);
        File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TransferWithANSIFileContentEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileContentEncoding = FileEncoding.ANSI,
            Action = DestinationAction.Append
        };

        Directory.CreateDirectory(destination.Directory);
        File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TransferWithUTF8WithBomFileContentEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileContentEncoding = FileEncoding.UTF8,
            EnableBomForContent = true,
            Action = DestinationAction.Append
        };

        Directory.CreateDirectory(destination.Directory);
        File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TransferWithUTF8WithoutBomFileContentEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileContentEncoding = FileEncoding.UTF8,
            EnableBomForContent = false,
            Action = DestinationAction.Append
        };

        Directory.CreateDirectory(destination.Directory);
        File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TransferWithWIN1252FileContentEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileContentEncoding = FileEncoding.WINDOWS1252,
            Action = DestinationAction.Append
        };

        Directory.CreateDirectory(destination.Directory);
        File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFiles_TransferWithOtherFileContentEncoding()
    {
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileContentEncoding = FileEncoding.Other,
            FileContentEncodingInString = "Windows-1252",
            Action = DestinationAction.Append
        };

        Directory.CreateDirectory(destination.Directory);
        File.Move(Path.Combine(_workDir, _source.FileName), Path.Combine(_destination.Directory, _source.FileName));

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }
}

