using Frends.SFTP.UploadFiles.Definitions;
using NUnit.Framework;
using Renci.SshNet;

namespace Frends.SFTP.UploadFiles.Tests.Lib
{
    public class UploadFilesTestBase
    {
        protected static Connection TestConnection { get; set; }

        protected static Source TestSource { get; set; }

        protected static Destination TestDestination { get; set; }

        protected static Options TestOptions { get; set; }

        protected static Info TestInfo { get; set; }

        protected static string TestWorkDir { get; set; }

        protected static string TestResultFile { get; set; } = "testResultFile.txt";

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            TestWorkDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");

            TestConnection = Helpers.GetSftpConnection();

            TestSource = new Source
            {
                Directory = TestWorkDir,
                FileName = "SFTPUploadTestFile1.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            TestDestination = new Destination
            {
                Directory = "/upload/Upload",
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
            };

            TestOptions = new Options
            {
                ThrowErrorOnFailure = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true,
            };

            TestInfo = new Info
            {
                WorkDir = null,
            };

            Directory.CreateDirectory(TestWorkDir);
            Helpers.CreateLargeDummyFile();
        }

        [SetUp]
        public void Setup()
        {
            CleanRemoteTestDirectory();
            Helpers.CreateDummyFiles(2);
        }

        [TearDown]
        public void TearDown()
        {
            CleanRemoteTestDirectory();
            Helpers.DeleteDummyFiles();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Directory.Delete(TestWorkDir, true);
        }

        private static void CleanRemoteTestDirectory()
        {
            using (var sftp = new SftpClient(TestConnection.Address, TestConnection.Port, TestConnection.UserName, TestConnection.Password))
            {
                sftp.Connect();
                Helpers.CleanDirectory(sftp, "/upload");
                sftp.Disconnect();
            }
        }
    }
}
