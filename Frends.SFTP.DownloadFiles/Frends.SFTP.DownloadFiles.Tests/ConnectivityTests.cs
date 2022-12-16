﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests;

[TestFixture]
public class ConnectivityTests : DownloadFilesTestBase
{
    [Test]
    public void DownloadFiles_TestWithLargerBuffer()
    {
        Helpers.UploadLargeTestFiles(_source.Directory, 1);

        var connection = Helpers.GetSftpConnection();
        connection.BufferSize = 256;

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "LargeTestFile1.bin",
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

        var connection = Helpers.GetSftpConnection();
        connection.Authentication = AuthenticationType.UsernamePasswordPrivateKeyString;
        connection.PrivateKeyFilePassphrase = "passphrase";
        connection.PrivateKeyString = key;

        var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestWithInteractiveKeyboardAuthentication()
    {
        var connection = Helpers.GetSftpConnection();
        connection.UseKeyboardInteractiveAuthentication = true;

        var result = SFTP.DownloadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestKeepAliveIntervalWithDefault()
    {
        Helpers.UploadLargeTestFiles(_source.Directory, 1);

        var connection = Helpers.GetSftpConnection();

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "*.bin",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        var result = SFTP.DownloadFiles(source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void DownloadFiles_TestKeepAliveIntervalWith1ms()
    {
        Helpers.UploadLargeTestFiles(_source.Directory, 1);

        var connection = Helpers.GetSftpConnection();
        connection.KeepAliveInterval = 1;
        connection.BufferSize = 256;

        var source = new Source
        {
            Directory = _source.Directory,
            FileName = "*.bin",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        var result = SFTP.DownloadFiles(source, _destination, connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }
}

