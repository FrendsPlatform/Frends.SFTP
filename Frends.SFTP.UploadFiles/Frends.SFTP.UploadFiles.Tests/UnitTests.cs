using NUnit.Framework;
using System.IO;
using System;
using System.Net;
using System.Text;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Renci.SshNet.Common;
using Frends.SFTP.UploadFiles.Definitions;
using System.Linq;

namespace Frends.SFTP.UploadFiles.Tests
{
    /// <summary>
    /// NOTE: To run these unit tests, you need an SFTP test server.
    /// This run command will create a docker container which is used in the tests.
    /// Run this command with absolute path to the Frends.SFTP.UploadFiles.Tests\Volumes diretory.
    /*   
        docker run -v $PWD/Volumes/ssh_host_rsa_key.pub:/home/foo/.ssh/keys/ssh_host_rsa_key.pub:ro -v $PWD/Volumes/share:/home/foo/share -p 2222:22 -d atmoz/sftp foo:pass:::upload
    */
    /// </summary>
    [TestFixture]
    class TestClass
    {
        /// <summary>
        /// Test credentials for docker server
        /// </summary>
        private static string _dockerAddress = Dns.GetHostName();
        private static string _dockerUsername = "foo";
        private static string _dockerPassword = "pass";

        /// <summary>
        /// Test credentials for HiQ test server
        /// </summary>
        private static string _HiQOpsAddress = Environment.GetEnvironmentVariable("HiQ_OpsTestSftpServerAddress");
        private static string _HiQOpsUsername = Environment.GetEnvironmentVariable("HiQ_OpsTestSftpServerUsername");
        private static string _HiQOpsPassword = Environment.GetEnvironmentVariable("HiQ_OpsTestSftpServerPassword");

        private static Connection _connection;
        private static Source _source;
        private static Destination _destination;
        private static Options _options;
        private static Info _info;
        private static string _workDir;
        private static string _testResultFile = "testResultFile.txt";

        [OneTimeSetUp]
        public static void Setup()
        {
            _workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");

            _connection = new Connection
            {
                ConnectionTimeout = 60,
                Address = _dockerAddress,
                Port = 2222,
                UserName = _dockerUsername,
                Authentication = AuthenticationType.UsernamePassword,
                Password = _dockerPassword,
                ServerFingerPrint = null,
                BufferSize = 32
            };

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

        [Test]
        public void UploadFiles_TestSimpleTransfer()
        {
            var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TestWithLargerBuffers()
        {
            var connection = new Connection
            {
                ConnectionTimeout = 60,
                Address = _dockerAddress,
                Port = 2222,
                UserName = _dockerUsername,
                Authentication = AuthenticationType.UsernamePassword,
                Password = _dockerPassword,
                BufferSize = 256
            };

            var source = new Source
            {
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/"),
                FileName = "LargeTestFile.bin",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var result = SFTP.UploadFiles(source, _destination, connection, _options, _info, new CancellationToken());
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
                Directory = _source.Directory,
                FileName = "*.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = SFTP.UploadFiles(source, destination, _connection, options, _info, new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(4, result.SuccessfulTransferCount);
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
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/"),
                FileName = "FileThatDontExist.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing,
            };

            var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken()));
            Assert.That(ex.Message.StartsWith("SFTP transfer failed:"));
        }

        [Test]
        public void UploadFiles_TestTransferThatThrowsWithIncorrectCredentials()
        {
            var connection = new Connection
            {
                ConnectionTimeout = 10,
                Address = _dockerAddress,
                Port = 22,
                UserName = "demo",
                Authentication = AuthenticationType.UsernamePassword,
                Password = "demo",
                BufferSize = 32
            };
            Assert.Throws<Exception>(() => SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        }

        [Test]
        public void UploadFiles_TestThrowsWithWrongPort()
        {
            var connection = new Connection
            {
                ConnectionTimeout = 10,
                Address = _dockerAddress,
                Port = 51651,
                UserName = _dockerUsername,
                Authentication = AuthenticationType.UsernamePassword,
                Password = _dockerPassword,
                BufferSize = 32
            };

            var ex = Assert.Throws<Exception>(() => SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
            Assert.That(ex.Message.StartsWith("SFTP transfer failed: Unable to establish the socket: No such host is known"));
        }

        [Test]
        public void UploadFiles_TestWithFileMaskWithFileAlreadyInDestination()
        {
            var source = new Source
            {
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/"),
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
            Assert.AreEqual(4, result.SuccessfulTransferCount);
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
            Assert.IsTrue(CheckFileExistsInDestination("/upload/Upload/SFTPUploadTestFile" + date.ToString(@"yyyy-MM-dd") + ".txt"));
        }

        [Test]
        public void UploadFiles_TestSourceDirectoryWithMacros()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/testfolder_%Year%");
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
        public void UploadFiles_TestAppendToExistingFile()
        {
            var result = SFTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var fullPath = _destination.Directory + "/" + _source.FileName;
            var content1 = GetTransferredFileContent(fullPath);

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
                Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/"),
                FileName = "SFTPUploadTestFile2.txt",
                Action = SourceAction.Error,
                Operation = SourceOperation.Nothing
            };

            result = SFTP.UploadFiles(source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            var content2 = GetTransferredFileContent(fullPath);
            Assert.AreNotEqual(content1.Length, content2.Length);
        }

        [Test]
        public void UploadFiles_TestPrivateKeyFileRsa()
        {
            var connection = new Connection
            {
                ConnectionTimeout = 60,
                Address = _dockerAddress,
                Port = 2222,
                UserName = _dockerUsername,
                Authentication = AuthenticationType.UsernamePasswordPrivateKeyFile,
                PrivateKeyFilePassphrase = "passphrase",
                PrivateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Volumes/ssh_host_rsa_key"),
                Password = _dockerPassword,
                BufferSize = 32
            };

            var result = SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFiles_TestPrivateKeyFileRsaFromString()
        {
            var key = @"-----BEGIN RSA PRIVATE KEY-----
Proc-Type: 4,ENCRYPTED
DEK-Info: DES-EDE3-CBC,C823E7CC4CBAC698

Fqxq2jbSKyb0a+oW96Tjoif3Kcb5zZ0FiQyiHgQozLXrecjdUwjWuedkDoZMxwG5
bxpOnxZ/88tDzYCtCPcYCPRF8BNueUsZO8/tztTra+4NgVd/omXHG5bqb7iMB4dc
2bIHmQ+xflefs8X72XjBUo1zG1TdacfrWag/b4D2Ftv0pOpbV1T1/T8a0IoVvaps
oxPT4mpHAfqtYH8sP/LE0V/EBgixdEW2Gqx8O1OuzyRPf2ZJKPGknlOtA8WDvAQh
xwFO+0jlSFwOqbf7kmFMmonLKaLT0HtJwf2GdMyVvuIu3rHkVFaVgyJ7AP4B8rSG
ccl9MmjGxUfgqqE3ea1VHR7XyW+QvpzKEPLmw46z6BGhJ9JtKPsx9QGVsYa4sPPI
xN0ZyK3iXvZFHljMltox+ISpgT5T+N1DaqU14//lYH5kK9N0/myszv9Ho5Y/gAS/
lxe1hX5+4ts7aauVxoZ1En3nhA3x/l+YI6wQ53a/a25HpATcL3wkFNZS0sYT1/Q/
8b3I4KUQkjjYcuCm6NCtjci+oP/Rfu+e02qkUXgy/vdRA7oUftSi4BAKj5qjD0d5
q9F6IvxLlghs08CC4z635wQr7EyxK/7EN1Ae1RmoA1E1/x8wLjMdGB5bV2EeYnwK
3CfAkT2iJ4M+uWPVULEZZTXGBFx82Ss0Qdo21n8lsPX1CUbkrUF1I40DI7VYwxll
6T1WHClGDVlS+eT3Yso6pDdV/FO6KHQ4ayEAEZl9c0F+4rtRfuGWh4KBW8LbNO6k
mpBJdqRTnmBsop9AHBWXQhljddmtt4/AKODIK0PBRdupsZv+SOmwq+g5ECnK8DGr
TxCdEqD6TYf4s0TVS+SXrb6396uvuMUOHEAk9ls088eQXkPqT/MXkuT/RKIu6J7i
qCQbuWCqoGAaGOeFvNm9YG+uoQzavr5dbEHWGywqNK9mM/uqCUdAruN78eRgr/Lg
Fj1Hoani9iXMhjGDMFbJnjO+47RzPIkTPGP+ExPADqshLA3NEvEOQjtdjTyTQlxw
iUTrMIWLKeGaTg7mUrDZZ9JelU6pFEld0j+jb7O9DMBLZdtndcHmHgkAoB9SwSk7
A5FlF8X3r6zogdrZqLBsUzSahCI5KU/HdQFyg4yKQPS+/Rg7czrTI5n2zLR+WGbQ
SFoeStnEW83JkoAk5qAaJELpKwzvxuNZfHFX8NxgaUnEKYNi+S0OdIpV7EOHwYCI
9v5bnl/XC6cvyve4+TuzbLg8gJX2eMD97jC016t+sO2BvmcL8ksEF1SZsSIHs8tO
ZmGPYiGDOCYdpfCff6JJPD4j5stUnjLGmEnXxEhoaHTQENg+z8ELg3X+vHsFsZTI
CNdxqEkvWXxHr0vEYSKAu/EMNQqB3YrvrKuIJez0acRwHZspzhT0fE384Itmnh39
o+w2UmYqEC7MQ3PQMPbnr/rhwywm1tboJVOmQaFaMkQGLea9wBvLylzBit3JX3Ku
OX7Q/wO4lqOlFhLtRnSL0cfuhRmt59pM75Zd+euX5tv9jmCj+AQT/kiBoMhNrDGk
N2gTujnH7HCr/afSBeL3xnYcEmeCQTxTPZofBjPC+TPd9g7MntSGBeU/Fstv0jbg
-----END RSA PRIVATE KEY-----
";
            var connection = new Connection
            {
                ConnectionTimeout = 60,
                Address = _dockerAddress,
                Port = 2222,
                UserName = _dockerUsername,
                Authentication = AuthenticationType.UsernamePasswordPrivateKeyString,
                PrivateKeyFilePassphrase = "passphrase",
                PrivateKeyString = key,
                Password = _dockerPassword,
                BufferSize = 32
            };

            var result = SFTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [TearDown]
        public void TearDown()
        {
            using (var sftp = new SftpClient(_connection.Address, _connection.Port, _connection.UserName, _connection.Password))
            {
                sftp.Connect();
                if (sftp.Exists(_destination.Directory))
                {
                    DeleteDirectory(sftp, _destination.Directory);
                }
                sftp.Disconnect();
                if (File.Exists(Path.Combine(_workDir, _testResultFile)))
                    File.Delete(Path.Combine(_workDir, _testResultFile));
            }
        }

        private bool CheckFileExistsInDestination(string fullpath)
        {
            using (var sftp = new SftpClient(_connection.Address, _connection.Port, _connection.UserName, _connection.Password))
            {
                var exists = true;
                sftp.Connect();
                if (!sftp.Exists(fullpath))
                    exists = false;
                sftp.Disconnect();
                return exists;
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
            if (client.Exists(path))
                client.DeleteDirectory(path);
        }

        private string GetTransferredFileContent(string fullPath)
        {
            var testfile = Path.Combine(_workDir, _testResultFile);
            using (var sftp = new SftpClient(_connection.Address, _connection.Port, _connection.UserName, _connection.Password))
            {
                sftp.Connect();

                sftp.ChangeDirectory(_destination.Directory);
                using (var file = File.OpenWrite(testfile))
                {
                    sftp.DownloadFile(fullPath, file);
                }
                sftp.Disconnect();
                sftp.Dispose();
            }

            return File.ReadAllText(testfile);
        }
    }
}
