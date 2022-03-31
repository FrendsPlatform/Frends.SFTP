using NUnit.Framework;
using System.IO;
using System;
using System.Net;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Frends.SFTP.ListFiles.Definitions;

namespace Frends.SFTP.ListFiles.Tests
{
    /// <summary>
    /// NOTE: To run these unit tests, you need an SFTP test server.
    /// 
    /// docker run -p 2222:22 -d atmoz/sftp foo:pass:::upload
    /// 
    /// </summary>
    [TestFixture]
    class TestClass
    {
        private static string _workDir;
        private static string _testFile1;
        private static string _testFile2;
        private static string _testFile3;
        private static string _testDataDir;
        private static Connection _connection;
        private static Options _options;

        [OneTimeSetUp]
        public static void OneTimeSetup()
        {
            _workDir = "/upload";
            _testFile1 = "TestFile1.txt";
            _testFile2 = "TestFile2.txt";
            _testFile3 = "TestFile3.csv";
            _testDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");

            _connection = new Connection
            {
                Address = Dns.GetHostName(),
                Port = 2222,
                UserName = "foo",
                Authentication = AuthenticationType.UsernamePassword,
                Password = "pass",
            };

            _options = new Options
            {
                Directory = "/upload",
                FileMask = "*.txt",
                IncludeType = IncludeType.File,
                IncludeSubdirectories = true
            };
        }

        [OneTimeSetUp]
        public void Setup()
        {
            UploadTestFiles();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            DeleteTestFiles();
        }

        [Test]
        public void ListFilesWithIncludeSubdirectoriesDisabled()
        {
            var options = new Options
            {
                Directory = "/upload",
                FileMask = "*.txt",
                IncludeType = IncludeType.File,
                IncludeSubdirectories = false
            };
            var result = SFTP.ListFiles(options, _connection, new CancellationToken());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ListFilesWithIncludeSubdirectoriesEnabled()
        {
            var result = SFTP.ListFiles(_options, _connection, new CancellationToken());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void ListFilesWithoutFileMask()
        {
            var options = new Options
            {
                Directory = "/upload",
                FileMask = "",
                IncludeType = IncludeType.File,
                IncludeSubdirectories = true
            };
            var result = SFTP.ListFiles(options, _connection, new CancellationToken());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void ListFilesWithIncludeTypeBoth()
        {
            var options = new Options
            {
                Directory = "/upload",
                FileMask = "",
                IncludeType = IncludeType.Both,
                IncludeSubdirectories = true
            };
            var result = SFTP.ListFiles(options, _connection, new CancellationToken());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(4));
        }

        [Test]
        public void ListFilesWithIncludeTypeDirectory()
        {
            var options = new Options
            {
                Directory = "/upload",
                FileMask = "",
                IncludeType = IncludeType.Directory,
                IncludeSubdirectories = true
            };
            var result = SFTP.ListFiles(options, _connection, new CancellationToken());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ListFilesThrowsWithIncorrectCredentials()
        {
            var connection = new Connection
            {
                Address = Dns.GetHostName(),
                Port = 2222,
                UserName = "demo",
                Authentication = AuthenticationType.UsernamePassword,
                Password = "demo",
            };
            var ex = Assert.Throws<Exception>(() => SFTP.ListFiles(_options, connection, new CancellationToken()));
            Assert.AreEqual("Authentication of SSH session failed: Permission denied (password).", ex.Message);
        }

        [Test]
        public void ListFilesThrowsWithInvalidConnectionOptions()
        {
            var connection = new Connection
            {
                Address = "foo.bar.com",
                Port = 1234,
                UserName = "demo",
                Authentication = AuthenticationType.UsernamePassword,
                Password = "demo",
            };
            var ex = Assert.Throws<Exception>(() => SFTP.ListFiles(_options, connection, new CancellationToken()));
            Assert.AreEqual("Unable to establish the socket: No such host is known.", ex.Message);
        }

        private static void UploadTestFiles()
        {
            var testfileFullPath = Path.Combine(_testDataDir, _testFile1);
            using (var sftp = new SftpClient(_connection.Address, _connection.Port, _connection.UserName, _connection.Password))
            {
                sftp.Connect();
                sftp.ChangeDirectory(_workDir);
                sftp.BufferSize = 1024;
                using (var fs = new FileStream(testfileFullPath, FileMode.Open))
                {
                    sftp.UploadFile(fs, _testFile1, true);
                    sftp.UploadFile(fs, _testFile3, true);
                }
                testfileFullPath = Path.Combine(_testDataDir, _testFile2);
                sftp.CreateDirectory("/upload/subdirectory");
                sftp.ChangeDirectory("/upload/subdirectory");
                using (var fs = new FileStream(testfileFullPath, FileMode.Open))
                {
                    sftp.UploadFile(fs, _testFile2, true);
                }
                sftp.Disconnect();
            }
        }

        private static void DeleteTestFiles()
        {
            using (var sftp = new SftpClient(_connection.Address, _connection.Port, _connection.UserName, _connection.Password))
            {
                sftp.Connect();
                sftp.ChangeDirectory(_workDir);
                var files = sftp.ListDirectory(".");
                foreach (var file in files)
                {
                    if (file.Name != "." && file.Name != "..")
                    {
                        if (file.IsDirectory)
                        {
                            sftp.ChangeDirectory(file.FullName);
                            foreach(var f in sftp.ListDirectory("."))
                            {
                                if (f.Name != "." && f.Name != "..")
                                {
                                    sftp.DeleteFile(f.Name);
                                }
                            }
                            sftp.ChangeDirectory(_workDir);
                            sftp.DeleteDirectory(file.FullName);
                        }
                        else
                        {
                            sftp.DeleteFile(file.FullName);
                        }
                    }
                }
                sftp.Disconnect();
            }
        }
    }
}
