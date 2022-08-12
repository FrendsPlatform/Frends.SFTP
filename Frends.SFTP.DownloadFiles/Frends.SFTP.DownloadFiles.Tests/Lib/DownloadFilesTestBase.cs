using System;
using System.IO;
using Renci.SshNet;
using Frends.SFTP.DownloadFiles.Definitions;
using NUnit.Framework;

namespace Frends.SFTP.DownloadFiles.Tests;

public class DownloadFilesTestBase
{
    protected static Connection _connection;
    protected static Source _source;
    protected static Destination _destination;
    protected static Options _options;
    protected static Info _info;
    protected static string _workDir;
    protected static string _destWorkDir;

    [OneTimeSetUp]
    public static void Setup()
    {
        _workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");
        _destWorkDir = Path.Combine(_workDir, "destination");

        _connection = Helpers.GetSftpConnection();

        _source = new Source
        {
            Directory = "/upload/Upload",
            FileName = "SFTPDownloadTestFile.txt",
            Action = SourceAction.Error,
            Operation = SourceOperation.Nothing,
        };

        _destination = new Destination
        {
            Directory = Path.Combine(_workDir, "destination"),
            Action = DestinationAction.Error,
            FileNameEncoding = FileEncoding.UTF8,
            EnableBomForFileName = true
        };

        _options = new Options
        {
            ThrowErrorOnFail = true,
            RenameSourceFileBeforeTransfer = true,
            RenameDestinationFileDuringTransfer = true,
            CreateDestinationDirectories = true,
            PreserveLastModified = false,
            OperationLog = true
        };

        _info = new Info
        {
            WorkDir = null,
        };
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            using (var sftp = new SftpClient(_connection.Address, _connection.Port, _connection.UserName, _connection.Password))
            {
                sftp.Connect();
                if (sftp.Exists(_source.Directory))
                    Helpers.DeleteDirectory(sftp, "/upload/");
                sftp.Disconnect();
            }
        } catch (Exception) { throw; }
        finally
        {
            if (Directory.Exists(_destWorkDir))
                Directory.Delete(_destWorkDir, true);
        }
    }
}

