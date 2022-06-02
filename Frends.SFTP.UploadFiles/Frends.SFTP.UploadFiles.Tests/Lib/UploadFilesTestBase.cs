using System;
using System.IO;
using Renci.SshNet;
using Frends.SFTP.UploadFiles.Definitions;
using NUnit.Framework;

namespace Frends.SFTP.UploadFiles.Tests
{
    public class UploadFilesTestBase
    {
        protected static Connection _connection;
        protected static Source _source;
        protected static Destination _destination;
        protected static Options _options;
        protected static Info _info;
        protected static string _workDir;
        protected static string _testResultFile = "testResultFile.txt";

        [OneTimeSetUp]
        public static void Setup()
        {
            _workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");

            _connection = Helpers.GetSftpConnection();

            _source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            _destination = new Destination
            {
                Directory = "/upload/Upload",
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
            using (var sftp = new SftpClient(_connection.Address, _connection.Port, _connection.UserName, _connection.Password))
            {
                sftp.Connect();
                if (sftp.Exists(_destination.Directory))
                {
                    Helpers.DeleteDirectory(sftp, _destination.Directory);
                }
                sftp.Disconnect();
            }
        }
    }
}
