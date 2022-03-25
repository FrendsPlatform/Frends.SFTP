using NUnit.Framework;
using System.IO;
using System;
using System.Net;
using System.Threading;
using Renci.SshNet;
using Frends.SFTP.WriteFile.Definitions;

namespace Frends.SFTP.WriteFile.Tests
{
    /// <summary>
    /// NOTE: To run these unit tests, you need an SFTP test server.
    /// 
    /// docker run -p 2222:22 -d atmoz/sftp foo:pass:::upload
    /// 
    /// </summary>
    [TestFixture]
    //[Ignore("Test needs new Dcoker based workflow")]
    class TestClass
    {
        private static string _workDir;
        private static Connection _param;
        private static Source _source;
        private static Destination _destination;

        [OneTimeSetUp]
        public static void Setup()
        {
            _workDir = "/upload";

            _param = new Connection
            {
                Address = Dns.GetHostName(),
                Port = 2222,
                UserName = "foo",
                Authentication = Enums.AuthenticationType.UsernamePassword,
                Password = "pass",
            };

            _source = new Source
            {
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/"),
                FileName = "TestFile1.txt"
            };

            _destination = new Destination
            {
                Directory = "/upload",
                Operation = Enums.DestinationOperation.Error
            };
        }

        [Test]
        public void WriteFile()
        {
            var result = SFTP.WriteFile(_source, _destination, _param, new CancellationToken());
            Assert.AreEqual(result.Success, true);
        }

        [Test]
        public void WriteFileThatExistsThrowsError()
        {
            SFTP.WriteFile(_source, _destination, _param, new CancellationToken());

            var ex = Assert.Throws<Exception>(() => SFTP.WriteFile(_source, _destination, _param, new CancellationToken()));
            Assert.That(ex.Message.Equals("Error in uploading the file: The destination file already exists."));
        }

        [Test]
        public void WriteFileWithOperationRename()
        {
            var destination = new Destination
            {
                Directory = "/upload",
                Operation = Enums.DestinationOperation.Rename
            };
            SFTP.WriteFile(_source, _destination, _param, new CancellationToken());
            var result1 = SFTP.WriteFile(_source, destination, _param, new CancellationToken());

            Assert.AreEqual(true, result1.Success);
            Assert.AreEqual(destination.Directory + "/TestFile1(1).txt", result1.DestinationPath);

            var result2 = SFTP.WriteFile(_source, destination, _param, new CancellationToken());

            Assert.AreEqual(true, result2.Success);
            Assert.AreEqual(destination.Directory + "/TestFile1(2).txt", result2.DestinationPath);
        }

        [Test]
        public void WriteFileWithOperationRenameWithCopyAlreadyInDestination()
        {
            var destination = new Destination
            {
                Directory = "/upload",
                Operation = Enums.DestinationOperation.Rename
            };

            var source = new Source
            {
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/"),
                FileName = "TestFile1(2).txt"
            };

            SFTP.WriteFile(_source, _destination, _param, new CancellationToken());
            SFTP.WriteFile(source, _destination, _param, new CancellationToken());

            var result = SFTP.WriteFile(_source, destination, _param, new CancellationToken());
            Assert.AreEqual(true, result.Success);
            Assert.AreEqual(destination.Directory + "/TestFile1(1).txt", result.DestinationPath);
        }

        [Test]
        public void WriteFileThrowsIfFileNotExist()
        {
            var source = new Source
            {
                Directory = "/",
                FileName = "FileThatDontExist.txt"
            };
            var destination = new Destination
            {
                Directory = "/upload",
                Operation = Enums.DestinationOperation.Error
            };
            var ex = Assert.Throws<FileNotFoundException>(() => SFTP.WriteFile(source, destination, _param, new CancellationToken()));
            Assert.That(ex.Message.StartsWith("Could not find file"));
        }

        [Test]
        public void WriteFileThrowsWithIncorrectCredentials()
        {
            var param = new Connection
            {
                Address = "foo.bar.com",
                Port = 1234,
                UserName = "demo",
                Authentication = Enums.AuthenticationType.UsernamePassword,
                Password = "demo",
            };
            var ex = Assert.Throws<Exception>(() => SFTP.WriteFile(_source, _destination, param, new CancellationToken()));
            Assert.AreEqual("Unable to establish the socket: No such host is known.", ex.Message);
        }

        [TearDown]
        public void TearDown()
        {
            using (var sftp = new SftpClient(_param.Address, _param.Port, _param.UserName, _param.Password))
            {
                sftp.Connect();
                DeleteDirectory(sftp, _workDir);
                sftp.Disconnect();
            }
        }

        private static void DeleteDirectory(SftpClient client, string path)
        {
            foreach (var file in client.ListDirectory(path))
            {
                if ((file.Name != ".") && (file.Name != ".."))
                {
                    if (file.IsDirectory)
                    {
                        DeleteDirectory(client, file.FullName);
                    }
                    else
                    {
                        client.DeleteFile(file.FullName);
                    }
                }
            }
        }
    }
}
