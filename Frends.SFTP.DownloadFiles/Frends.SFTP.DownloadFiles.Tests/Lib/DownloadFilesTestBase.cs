using System;
using System.IO;
using Renci.SshNet;
using Frends.SFTP.DownloadFiles.Definitions;
using NUnit.Framework;

namespace Frends.SFTP.DownloadFiles.Tests
{
    public class DownloadFilesTestBase
    {
        protected Connection _connection;
        protected Source _source;
        protected Destination _destination;
        protected Options _options;
        protected Info _info;
        protected string _workDir;
        protected string _destWorkDir;

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            _workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");
            _destWorkDir = Path.Combine(_workDir, "destination");

            _connection = Helpers.GetSftpConnection();

            _source = new Source
            {
                Directory = "./upload/Upload",
                FileName = "SFTPDownloadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
                IncludeSubdirectories = false,
            };

            _destination = new Destination
            {
                Directory = Path.Combine(_workDir, "destination"),
                Action = DestinationAction.Error
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

        [SetUp]
        public virtual void Setup()
        {
            Helpers.UploadTestFiles(_source.Directory, 3);
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
                        Helpers.DeleteDirectory(sftp, "./upload/");
                    sftp.Disconnect();
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (Directory.Exists(_destWorkDir))
                    Directory.Delete(_destWorkDir, true);
            }

            Helpers.DeleteDummyFiles();
        }
    }
}



