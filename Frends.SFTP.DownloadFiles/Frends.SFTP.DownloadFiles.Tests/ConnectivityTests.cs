﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using System.Net.Sockets;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests;

[TestFixture]
public class ConnectivityTests : DownloadFilesTestBase
{
    [Test]
    public void DownloadFiles_TestWithLargerBuffer()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, "LargeTestFile.bin") }, _source.Directory);

        var connection = Helpers.GetSftpConnection();
        connection.BufferSize = 256;

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "LargeTestFile.bin",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        var result = SFTP.DownloadFiles(source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestTransferThatThrowsWithIncorrectCredentials()
    {
        var connection = Helpers.GetSftpConnection();
        connection.ConnectionTimeout = 10;
        connection.UserName = "demo";
        connection.Password = "demo";

        var result = Assert.Throws<Exception>(() => SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.That(result.Message.StartsWith("SFTP transfer failed: Authentication of SSH session failed: Permission denied (password)"));
    }

    [Test]
    public void DownloadFiles_TestPrivateKeyFileRsa()
    {
        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key");

        var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestPrivateKeyFileRsaFromString()
    {
        var key = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"));

        Helpers.UploadTestFiles(new List<string> { Path.Combine(_workDir, _source.FileName) }, _source.Directory);

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyString = key;

        var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void TestWithAzureBlobStorage()
    {
        var connection = new Connection()
        {
            ConnectionTimeout = 60,
            Address = "frendstasktestsftp.blob.core.windows.net",
            UserName = "frendstasktestsftp.tasktestuser",
            Password = "zB2lciR4Q3+s3OoJpTB+LX2iXEDFsq39R+uHVsIzLZB67m6bdkUzv8rXvP0Xzc2qL4vqgK3Tn9jRB143vFktcA==",
        };

        var source = new Source()
        {
            Directory = "/download",
            FileName = "downloadfile.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing
        };

        var result = SFTP.DownloadFiles(source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }
}

