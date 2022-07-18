using NUnit.Framework;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using Renci.SshNet;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests;

[TestFixture]
internal class MacroTests : DownloadFilesTestBase
{
    [Test]
    public void DownloadFiles_TestUsingMacros()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
        var destination = new Destination
        {
            Directory = _destination.Directory,
            FileName = "%SourceFileName%%Date%%SourceFileExtension%",
            Action = DestinationAction.Error,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        var date = DateTime.Now;
        Assert.IsTrue(File.Exists(Path.Combine(_destWorkDir, "SFTPDownloadTestFile" + date.ToString(@"yyyy-MM-dd") + ".txt")));
    }

    [Test]
    public void DownloadFiles_TestSourceDirectoryWithMacros()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory + "/testfolder_2022");

        var source = new Source
        {
            Directory = "/upload/Upload/testfolder_%Year%",
            FileName = _source.FileName,
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

    }

    [Test]
    public void DownloadFiles_TestDestinationDirectoryWithMacros()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);
        var year = DateTime.Now.Year.ToString();
        var destination = new Destination
        {
            Directory = Path.Combine(_destWorkDir, "test%Year%"),
            FileName = "",
            Action = DestinationAction.Error,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        var result = SFTP.DownloadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        var dir = Path.Combine(_destWorkDir, "test" + year);

        var exists = File.Exists(Path.Combine(dir, _source.FileName));
        Assert.IsTrue(exists);
    }

    [Test]
    public void DownloadFiles_TestSourceFileMoveWithMacros()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var year = DateTime.Now.Year.ToString();
        var to = "/upload/Upload/" + year + "_uploaded";
        var connection = Helpers.GetSftpConnection();
        using (var client = new SftpClient(connection.Address, connection.Port, connection.UserName, connection.Password))
        {
            client.Connect();
            Helpers.CreateSourceDirectories(client, to);
            client.Disconnect();
        }

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = _source.FileName,
            Action = SourceAction.Error,
            Operation = SourceOperation.Move,
            DirectoryToMoveAfterTransfer = "/upload/Upload/%Year%_uploaded"
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.SourceFileExists(to + "/" + _source.FileName));
        using (var client = new SftpClient(connection.Address, connection.Port, connection.UserName, connection.Password))
        {
            client.Connect();
            Helpers.DeleteDirectory(client, to);
            client.Disconnect();
        }
    }

    [Test]
    public void DownloadFiles_TestSourceFileRenameWithMacros()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = _source.FileName,
            Action = SourceAction.Error,
            Operation = SourceOperation.Rename,
            FileNameAfterTransfer = "uploaded_%SourceFileName%%SourceFileExtension%"
        };

        var result = SFTP.DownloadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);

        Assert.IsTrue(Helpers.SourceFileExists(_source.Directory + "/uploaded_" + source.FileName));
    }
}

