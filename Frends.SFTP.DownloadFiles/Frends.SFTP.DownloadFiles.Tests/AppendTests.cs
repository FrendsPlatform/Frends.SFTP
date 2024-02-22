using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    [TestFixture]
    class AppendTests : DownloadFilesTestBase
    {
        [Test]
        public async Task DownloadFiles_TestAppendToExistingFile()
        {
            Directory.CreateDirectory(_destWorkDir);
            File.Copy(Path.Combine(_workDir, _source.FileName), Path.Combine(_destWorkDir, _source.FileName));
            var file1 = new FileInfo(Path.Combine(_workDir, _source.FileName));

            var destination = new Destination
            {
                Directory = _destWorkDir,
                FileName = _source.FileName,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
                Action = DestinationAction.Append,
                AddNewLine = true,
                FileContentEncoding = FileEncoding.UTF8,
                EnableBomForContent = true
            };

            var source = new Source
            {
                Directory = "/upload/Upload",
                FileName = "SFTPDownloadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.DownloadFiles(source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var file2 = new FileInfo(Path.Combine(_destWorkDir, _source.FileName));
            Assert.AreNotEqual(file1.Length, file2.Length);
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, destination.FileName)));
        }

        [Test]
        public async Task DownloadFiles_AppendingToExistingFile()
        {
            Directory.CreateDirectory(_destWorkDir);

            Console.WriteLine(_connection.UserName);
            Console.WriteLine(_connection.Password);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var destination = new Destination
            {
                Directory = _destWorkDir,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
                Action = DestinationAction.Append,
                AddNewLine = true,
                FileContentEncoding = FileEncoding.UTF8,
                EnableBomForContent = true
            };

            var result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content1 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));

            result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
            Assert.IsTrue(content2.Length > content1.Length);
            Assert.AreEqual(content1 + Environment.NewLine + content1, content2);
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_AppendingToExistingFileRenameSourceFile()
        {
            Directory.CreateDirectory(_destWorkDir);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var destination = new Destination
            {
                Directory = _destWorkDir,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
                Action = DestinationAction.Append,
                AddNewLine = true,
                FileContentEncoding = FileEncoding.UTF8,
                EnableBomForContent = true
            };
            var result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content1 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));

            result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
            Assert.IsTrue(content2.Length > content1.Length);
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_AppendingToExistingFileRenameDestinationFile()
        {
            Directory.CreateDirectory(_destWorkDir);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var destination = new Destination
            {
                Directory = _destWorkDir,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
                Action = DestinationAction.Append,
                AddNewLine = true,
                FileContentEncoding = FileEncoding.UTF8,
                EnableBomForContent = true
            };
            var result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content1 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));

            result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
            Assert.IsTrue(content2.Length > content1.Length);
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_AppendingToExistingFileRenameBoth()
        {
            Directory.CreateDirectory(_destWorkDir);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var destination = new Destination
            {
                Directory = _destWorkDir,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
                Action = DestinationAction.Append,
                AddNewLine = true,
                FileContentEncoding = FileEncoding.UTF8,
                EnableBomForContent = true
            };

            var result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content1 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));

            result = await SFTP.DownloadFiles(_source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
            Assert.IsTrue(content2.Length > content1.Length);
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_AppendingToExistingFileRenameBothWithSourceFileNameStar()
        {
            Directory.CreateDirectory(_destWorkDir);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = true,
                RenameDestinationFileDuringTransfer = true,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var destination = new Destination
            {
                Directory = _destWorkDir,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
                Action = DestinationAction.Append,
                AddNewLine = true,
                FileContentEncoding = FileEncoding.UTF8,
                EnableBomForContent = true
            };

            var source = new Source
            {
                Directory = "/upload/Upload",
                FileName = "*",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.DownloadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content1 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));

            result = await SFTP.DownloadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
            Assert.IsTrue(content2.Length > content1.Length);
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
        }

        [Test]
        public async Task DownloadFiles_AppendWithoutNewLine()
        {
            Directory.CreateDirectory(_destWorkDir);

            var options = new Options
            {
                ThrowErrorOnFail = true,
                RenameSourceFileBeforeTransfer = false,
                RenameDestinationFileDuringTransfer = false,
                CreateDestinationDirectories = true,
                PreserveLastModified = true,
                OperationLog = true
            };

            var destination = new Destination
            {
                Directory = _destWorkDir,
                FileNameEncoding = FileEncoding.UTF8,
                EnableBomForFileName = true,
                Action = DestinationAction.Append,
                AddNewLine = false,
                FileContentEncoding = FileEncoding.UTF8,
                EnableBomForContent = true
            };

            var source = new Source
            {
                Directory = "/upload/Upload",
                FileName = "*",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = await SFTP.DownloadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content1 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));

            result = await SFTP.DownloadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = File.ReadAllText(Path.Combine(destination.Directory, _source.FileName));
            Assert.IsTrue(content2.Length > content1.Length);
            Assert.IsTrue(File.Exists(Path.Combine(destination.Directory, _source.FileName)));
        }
    }
}


