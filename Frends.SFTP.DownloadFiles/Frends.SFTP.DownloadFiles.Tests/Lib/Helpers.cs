using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    internal static class Helpers
    {
        /// <summary>
        /// Test credentials for docker server.
        /// </summary>
        readonly static string _dockerAddress = "localhost";
        readonly static string _dockerUsername = "foo";
        readonly static string _dockerPassword = "pass";
        readonly static string _baseDir = "/upload/";
        readonly static string _workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");

        internal static Connection GetSftpConnection()
        {
            var connection = new Connection
            {
                ConnectionTimeout = 60,
                Address = _dockerAddress,
                Port = 2222,
                UserName = _dockerUsername,
                Authentication = AuthenticationType.UsernamePassword,
                Password = _dockerPassword,
                BufferSize = 32,
                KeepAliveInterval = -1
            };

            return connection;
        }

        internal static void DeleteDirectory(SftpClient client, string dir)
        {

            foreach (var file in client.ListDirectory(dir))
            {
                if ((file.Name != ".") && (file.Name != ".."))
                {
                    if (file.IsDirectory) DeleteDirectory(client, file.FullName);
                    else client.DeleteFile(file.FullName);
                }
            }
            if (client.Exists(dir) && !dir.Equals(_baseDir)) client.DeleteDirectory(dir);
        }

        internal static string[] UploadTestFiles(string destination, int count = 3, string to = null, List<string> filenames = null)
        {
            var filePaths = new List<string>();

            var files = (filenames == null) ? CreateDummyFiles(count) : CreateDummyFiles(0, filenames);
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();
                if (client.Exists(destination))
                    DeleteDirectory(client, destination);
                CreateSourceDirectories(client, destination);
                client.ChangeDirectory(destination);
                if (!string.IsNullOrEmpty(to)) client.CreateDirectory(to);
                foreach (var file in files)
                {
                    using (var fs = File.OpenRead(file))
                    {
                        client.UploadFile(fs, Path.GetFileName(file), true);
                    }
                    filePaths.Add(Path.Combine(destination, Path.GetFileName(file)).Replace("\\", "/"));

                }
                client.Disconnect();
            }

            return filePaths.ToArray();
        }

        internal static string[] UploadLargeTestFiles(string destination, int count = 3, string to = null)
        {
            var filePaths = new List<string>();

            var files = CreateLargeDummyFiles(count);
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();
                if (client.Exists(destination))
                    DeleteDirectory(client, destination);
                CreateSourceDirectories(client, destination);
                client.ChangeDirectory(destination);
                if (!string.IsNullOrEmpty(to)) client.CreateDirectory(to);
                foreach (var file in files)
                {
                    using (var fs = File.OpenRead(file))
                    {
                        client.UploadFile(fs, Path.GetFileName(file), true);
                        filePaths.Add(Path.Combine(destination, Path.GetFileName(file).Replace("\\", "/")));
                    }

                }
                client.Disconnect();
            }

            return filePaths.ToArray();
        }

        internal static void DeleteRemoteFiles(int count, string directory)
        {
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();
                var files = client.ListDirectory(directory);
                var i = 0;
                foreach (var file in files)
                {
                    if (i == count)
                        break;
                    client.DeleteFile(file.FullName);
                    i++;
                }
            }
        }

        internal static List<string> CreateDummyFiles(int count, List<string> filenames = null)
        {
            Directory.CreateDirectory(_workDir);
            var filePaths = new List<string>();
            if (filenames == null)
            {
                var name = "SFTPDownloadTestFile";
                var extension = ".txt";
                for (var i = 1; i <= count; i++)
                {
                    var path = Path.Combine(_workDir, name + i + extension);
                    File.WriteAllText(path, "This is a test file.");
                    filePaths.Add(path);
                }
            }
            else
            {
                foreach (var filename in filenames)
                {
                    var path = Path.Combine(_workDir, filename);
                    File.WriteAllText(path, "This is a test file.");
                    filePaths.Add(path);
                }
            }

            return filePaths;
        }

        internal static List<string> CreateLargeDummyFiles(int count = 1)
        {
            var filePaths = new List<string>();
            var name = "LargeTestFile";
            var extension = ".bin";
            for (var i = 1; i <= count; i++)
            {
                var path = Path.Combine(_workDir, name + i + extension);
                var fs = new FileStream(path, FileMode.CreateNew);
                fs.Seek(2048L * 1024 * 100, SeekOrigin.Begin);
                fs.WriteByte(0);
                fs.Close();
                filePaths.Add(path);
            }

            return filePaths;
        }

        internal static void DeleteDummyFiles()
        {
            foreach (var file in Directory.EnumerateFileSystemEntries(_workDir))
            {
                if (Directory.Exists(file))
                    Directory.Delete(file, true);
                File.Delete(file);
            }
        }

        internal static void CreateSourceDirectories(SftpClient client, string path)
        {
            // Consistent forward slashes
            foreach (string dir in path.Replace(@"\", "/").Split('/'))
            {
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    if (!TryToChangeDir(client, dir) && ("/" + dir != client.WorkingDirectory))
                    {
                        client.CreateDirectory(dir);
                        client.ChangeDirectory(dir);
                    }
                }
            }
        }

        private static bool TryToChangeDir(SftpClient client, string dir)
        {
            try
            {
                client.ChangeDirectory(dir);
                return true;
            }
            catch { return false; }
        }

        internal static bool SourceFileExists(string path)
        {
            bool exists;
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();
                exists = client.Exists(path);
                client.Disconnect();
            }
            return exists;
        }

        internal static void CreateSubDirectory(string path)
        {
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();
                if (!client.Exists(path))
                    client.CreateDirectory(path);
                client.Disconnect();
            }
        }

        internal static void DeleteSubDirectory(string path)
        {
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();
                client.DeleteDirectory(path);
                client.Disconnect();
            }
        }

        internal static void SetTestFileLastModified(string path, DateTime date)
        {
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();
                var attributes = client.GetAttributes(path);
                attributes.LastWriteTime = date;
                client.SetAttributes(path, attributes);
                client.Disconnect();
            }
        }

        internal static Tuple<byte[], byte[]> GetServerFingerPrintAndHostKey()
        {
            Tuple<byte[], byte[]> result = null;
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.ConnectionInfo.HostKeyAlgorithms.Clear();
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-rsa", (data) => { return new KeyHostAlgorithm("ssh-rsa", new RsaKey(), data); });

                client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                {
                    result = new Tuple<byte[], byte[]>(e.FingerPrint, e.HostKey);
                    e.CanTrust = true;
                };
                client.Connect();
                client.Disconnect();
            }
            return result;
        }

        internal static string ConvertToMD5Hex(byte[] fingerPrint)
        {
            return BitConverter.ToString(fingerPrint).Replace("-", ":");
        }

        internal static string ConvertToSHA256Hash(byte[] hostKey)
        {
            var fingerprint = "";
            using (SHA256 mySHA256 = SHA256.Create())
            {
                fingerprint = Convert.ToBase64String(mySHA256.ComputeHash(hostKey));
            }
            return fingerprint;
        }

        internal static string ConvertToSHA256Hex(byte[] hostKey)
        {
            var fingerprint = "";
            using (SHA256 mySHA256 = SHA256.Create())
            {
                fingerprint = ToHex(mySHA256.ComputeHash(hostKey));
            }
            return fingerprint;
        }

        internal static string ToHex(byte[] bytes)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString("x2"));
            return result.ToString();
        }
    }
}



