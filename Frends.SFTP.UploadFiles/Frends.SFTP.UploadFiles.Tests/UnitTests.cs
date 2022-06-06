using NUnit.Framework;
using System.IO;
using System;
using System.Threading;
using Frends.SFTP.UploadFiles.Definitions;


namespace Frends.SFTP.UploadFiles.Tests
{
    /// <summary>
    /// NOTE: To run these unit tests, you need an SFTP test server.
    /// This run command will create a docker container which is used in the tests.
    /// Run this command with absolute path to the Frends.SFTP.UploadFiles.Tests\Volumes diretory.
    /// docker-compose -f .\Frends.SFTP.UploadFiles.Tests\docker_compose.yml up -d
    /// </summary>
    [TestFixture]
    class TestClass : UploadFilesTestBase
    {

        [Test]
        public void UploadFiles_TestSimpleTransfer()
        {
            var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test] 
        public void UploadFiles_TestWithOperationLogDisabled()
        {
            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = false
            };

            var result = SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.OperationsLog.Count);
        }

        [Test]
        public void UploadFiles_TestWithMultipleSubdirectoriesInDestination()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload/sub",
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
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
        public void UploadFiles_TestOneErrorInTransferWithMultipleFiles()
        {
            var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var options = new Options
            {
                ThrowErrorOnFail = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var source = new Source
            {
                Directory = _workDir,
                FileName = "*.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = SFTP.UploadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.AreEqual(1, result.FailedTransferCount);
        }

        [Test]
        public void UploadFile_TestSingleFileTransferWithError()
        {
            var options = new Options
            {
                ThrowErrorOnFail = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };
            var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);

            result = SFTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.That(result.FailedTransferCount == 1);

        }

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
        public void UploadFiles_TestWithFileMaskWithFileAlreadyInDestination()
        {
            var source = new Source
            {
                Directory = _workDir,
                FileName = "*File.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var options = new Options
            {
                ThrowErrorOnFail = false,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = false,
                OperationLog = true
            };

            var result = SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);

            source.FileName = "*.txt";
            result = SFTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FailedTransferCount);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TestUsingMacros()
        {
            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "%SourceFileName%%Date%%SourceFileExtension%",
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var date = DateTime.Now;
            Assert.IsTrue(Helpers.CheckFileExistsInDestination("/upload/Upload/SFTPUploadTestFile" + date.ToString(@"yyyy-MM-dd") + ".txt"));
        }

        [Test]
        public void UploadFiles_TestSourceDirectoryWithMacros()
        {
            var path = Path.Combine(_workDir, "testfolder_%Year%");
            var source = new Source
            {
                Directory = path,
                FileName = "*.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            var result = SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TestDestinationDirectoryWithMacros()
        {
            var destination = new Destination
            {
                Directory = "upload/Upload/test%Year%",
                FileName = "",
                Action = DestinationAction.Error,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var result = SFTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            var year = DateTime.Now.Year.ToString();
            Assert.That(Helpers.CheckFileExistsInDestination("upload/Upload/test" + year));

        }

        [Test]
        public void UploadFiles_TestAppendToExistingFile()
        {
            var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var fullPath = _destination.Directory + "/" + _source.FileName;
            var content1 = Helpers.GetTransferredFileContent(fullPath);

            var destination = new Destination
            {
                Directory = "/upload/Upload",
                FileName = "SFTPUploadTestFile.txt",
                Action = DestinationAction.Append,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true
            };

            var source = new Source
            {
                Directory = _workDir,
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = SFTP.UploadFiles(source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = Helpers.GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1, content2);
        }
    }
}
