using NUnit.Framework;
using System.IO;
using System;
using System.Threading;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests;

[TestFixture]
class ErrorTesting : UploadFilesTestBase
{

    [Test]
    public void UploadFiles_TestTransferThatThrowsIfFileNotExist()
    {
        var source = new Source
        {
            Directory = _workDir,
            FileName = "FileThatDontExist.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("SFTP transfer failed:"));
    }

    [Test]
    public void UploadFiles_TestTransferThatExistsThrowsError()
    {
        var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);

        var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith($"SFTP transfer failed: 1 Errors: Failure in CheckIfDestination"));
    }

    [Test]
    public void UploadFiles_TestThrowsWithWrongPort()
    {
        var connection = Helpers.GetSftpConnection();
        connection.Port = 51651;

        var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.That(ex.Message.StartsWith("SFTP transfer failed: Unable to establish the socket: No such host is known"));
    }
}

