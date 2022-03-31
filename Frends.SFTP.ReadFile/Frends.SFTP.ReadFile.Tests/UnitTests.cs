using NUnit.Framework;
using System.IO;
using System;
using System.Net;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;
using Frends.SFTP.ReadFile.Definitions;

namespace Frends.SFTP.ReadFile.Tests
{
    /// <summary>
    /// NOTE: To run these unit tests, you need an SFTP test server.
    /// 
    /// docker run -p 2222:22 -d atmoz/sftp foo:pass:::upload
    /// 
    /// </summary>
    [TestFixture]
    [Ignore("Test needs new Docker based workflow")]
    class TestClass
    {
        private static string _workDir;
        private static string _testDataDir;
        private static string _localDownloadDir;
        private static Connection _param;
        private static Source _source;
        private static Destination _destination;

        [OneTimeSetUp]
        public static void OneTimeSetup()
        {
            _workDir = "/upload";
            _testDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");
            _localDownloadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/Downloads");

            _param = new Connection
            {
                Address = Dns.GetHostName(),
                Port = 2222,
                UserName = "foo",
                Authentication = AuthenticationType.UsernamePassword,
                Password = "pass",
            };

            _source = new Source
            {
                Directory = "/upload",
                FileName = "TestFile1.txt"
            };

            _destination = new Destination
            {
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/Downloads/"),
                Operation = DestinationOperation.Error
            };
        }

        [SetUp]
        public void Setup()
        {
            DeleteTestFilesFromSftp();
            DeleteLocalTestFiles();
            UploadTestFiles();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteTestFilesFromSftp();
            DeleteLocalTestFiles();
        }

        [Test]
        public void ReadFile()
        {
            var result = SFTP.ReadFile(_source, _destination, _param, new CancellationToken());
            Assert.AreEqual(true, result.Success);
        }

        [Test]
        public void ReadFileWithOperationRename()
        {
            var destination = new Destination
            {
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/Downloads/"),
                Operation = DestinationOperation.Rename
            };
            SFTP.ReadFile(_source, _destination, _param, new CancellationToken());
            var result = SFTP.ReadFile(_source, destination, _param, new CancellationToken());
            Assert.AreEqual(true, result.Success);
            Assert.AreEqual("TestFile1(1).txt", Path.GetFileName(result.DestinationPath));
        }

        [Test]
        public void ReadFileWithOperationRenameWithCopyAlreadyInDestination()
        {
            var source = new Source
            {
                Directory = "/upload",
                FileName = "TestFile1(2).txt"
            };

            var destination = new Destination
            {
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/Downloads/"),
                Operation = DestinationOperation.Rename
            };

            // Downloading TestFile1.txt
            SFTP.ReadFile(_source, _destination, _param, new CancellationToken());

            // Downloading TestFile1(2).txt
            SFTP.ReadFile(source, _destination, _param, new CancellationToken());

            // Downloading TestFile1.txt again
            var result = SFTP.ReadFile(_source, destination, _param, new CancellationToken());

            Assert.AreEqual(true, result.Success);
            Assert.AreEqual("TestFile1(1).txt", Path.GetFileName(result.DestinationPath));
        }

        [Test]
        public void ReadFileThrowsWithOperationError()
        {
            SFTP.ReadFile(_source, _destination, _param, new CancellationToken());
            var ex = Assert.Throws<Exception>(() => SFTP.ReadFile(_source, _destination, _param, new CancellationToken()));
            Assert.That(ex.Message.Equals("Error in downloading the file: The destination file already exists."));
        }

        [Test]
        public void ReadFileThrowsIfFileNotExist()
        {
            var source = new Source
            {
                Directory = "/upload",
                FileName = "FileThatDontExist.txt"
            };

            var ex = Assert.Throws<SftpPathNotFoundException>(() => SFTP.ReadFile(source, _destination, _param, new CancellationToken()));
            Assert.That(ex.Message.Equals("No such file"));
        }

        [Test]
        public void ReadFileThrowsWithIncorrectCredentials()
        {
            var param = new Connection
            {
                Address = "foo.bar.com",
                Port = 1234,
                UserName = "demo",
                Authentication = AuthenticationType.UsernamePassword,
                Password = "demo",
            };
            var ex = Assert.Throws<Exception>(() => SFTP.ReadFile(_source, _destination, param, new CancellationToken()));
            Assert.AreEqual("Unable to establish the socket: No such host is known.", ex.Message);
        }

        private static void UploadTestFiles()
        {
            using (var sftp = new SftpClient(_param.Address, _param.Port, _param.UserName, _param.Password))
            {
                sftp.Connect();
                sftp.ChangeDirectory(_workDir);
                sftp.BufferSize = 1024;
                foreach (var file in Directory.GetFiles(_testDataDir, "*", SearchOption.TopDirectoryOnly))
                {
                    using (FileStream fs = new FileStream(file, FileMode.Open))
                    {
                        sftp.UploadFile(fs, Path.GetFileName(file), true);
                    }
                }
                sftp.Disconnect();
            }
        }

        private static void DeleteLocalTestFiles()
        {
            var info = new DirectoryInfo(_localDownloadDir);
            foreach (var file in info.EnumerateFiles())
            {
                file.Delete();
            }
        }

        private static void DeleteTestFilesFromSftp()
        {
            using (var client = new SftpClient(_param.Address, _param.Port, _param.UserName, _param.Password)){
                client.Connect();
                foreach (var file in client.ListDirectory(_workDir))
                {
                    if ((file.Name != ".") && (file.Name != ".."))
                    {
                        client.DeleteFile(file.FullName);
                    }
                }
                client.Disconnect();
            }
        }
    }
}
