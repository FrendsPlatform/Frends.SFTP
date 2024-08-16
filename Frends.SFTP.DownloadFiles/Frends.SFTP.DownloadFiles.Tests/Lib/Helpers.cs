using System;
using System.Linq;
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
        internal readonly static string _dockerAddress = "localhost";
        internal static string _dockerUser;
        internal static string _dockerPwd;
        internal static string _dockerPass;
        internal readonly static string _baseDir = "./upload/";
        internal readonly static string _workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");

        private static void GetCredentialsFromEnvFile()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../.env");
            if (!File.Exists(path))
                return;

            foreach (var parts in File.ReadAllLines(path).Where(e => e.Split('=').Length == 2).Select(e => e.Split('=')))
            {
                switch (parts[0])
                {
                    case "USER":
                        _dockerUser = parts[1];
                        break;
                    case "PASS":
                        _dockerPwd = parts[1];
                        break;
                    case "PASSPHRASE":
                        _dockerPass = parts[1];
                        break;
                }
            }
        }

        internal static Connection GetSftpConnection()
        {
            GetCredentialsFromEnvFile();

            var connection = new Connection
            {
                ConnectionTimeout = 60,
                Address = _dockerAddress,
                Port = 2222,
                UserName = _dockerUser,
                Authentication = AuthenticationType.UsernamePassword,
                Password = _dockerPwd,
                BufferSize = 32,
                KeepAliveInterval = -1,
                PrivateKeyPassphrase = _dockerPass,
            };

            return connection;
        }

        internal static void DeleteDirectory(SftpClient client, string dir)
        {

            foreach (var file in client.ListDirectory(dir).Where(e => e.Name != "." && e.Name != ".."))
            {
                if (file.IsDirectory)
                    DeleteDirectory(client, file.FullName);
                else
                    client.DeleteFile(file.FullName);
            }
            if (client.Exists(dir) && !dir.Equals(_baseDir))
                client.DeleteDirectory(dir);
        }

        internal static string[] UploadTestFiles(string destination, int count = 3, string to = null, List<string> filenames = null)
        {
            var filePaths = new List<string>();

            var files = (filenames == null) ? CreateDummyFiles(count) : CreateDummyFiles(0, filenames);
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUser, _dockerPwd))
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
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUser, _dockerPwd))
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
                foreach (var path in filenames.Select(e => Path.Combine(_workDir, e)))
                {
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
            var origPath = client.WorkingDirectory;
            // Consistent forward slashes
            foreach (string dir in path.Replace(@"\", "/").Split('/').Where(e => !string.IsNullOrWhiteSpace(e) && !TryToChangeDir(client, e) && ("/" + e != client.WorkingDirectory)))
            {
                client.CreateDirectory(dir);
                client.ChangeDirectory(dir);
            }

            client.ChangeDirectory(origPath);
        }

        private static bool TryToChangeDir(SftpClient client, string dir)
        {
            try
            {
                client.ChangeDirectory(dir);
                return true;
            }
            catch (SftpPathNotFoundException)
            {
                return false;
            }
        }

        internal static bool SourceFileExists(string path)
        {
            bool exists;
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUser, _dockerPwd))
            {
                client.Connect();
                exists = client.Exists(path);
                client.Disconnect();
            }
            return exists;
        }

        internal static void CreateSubDirectory(string path)
        {
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUser, _dockerPwd))
            {
                client.Connect();
                if (!client.Exists(path))
                    client.CreateDirectory(path);
                client.Disconnect();
            }
        }

        internal static void DeleteSubDirectory(string path)
        {
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUser, _dockerPwd))
            {
                client.Connect();
                client.DeleteDirectory(path);
                client.Disconnect();
            }
        }

        internal static void SetTestFileLastModified(string path, DateTime date)
        {
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUser, _dockerPwd))
            {
                client.Connect();
                var attributes = client.GetAttributes(path);
                attributes.LastWriteTime = date;
                client.SetAttributes(path, attributes);
                client.Disconnect();
            }
        }

        internal static Tuple<string, string, byte[]> GetServerFingerPrintsAndHostKey()
        {
            Tuple<string, string, byte[]> result = null;
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUser, _dockerPwd))
            {
                client.ConnectionInfo.HostKeyAlgorithms.Clear();
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-rsa", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);
                    return new KeyHostAlgorithm("ssh-rsa", new RsaKey(sshKeyData)); 
                });

                client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                {
                    result = new Tuple<string, string, byte[]>(e.FingerPrintMD5, e.FingerPrintSHA256, e.HostKey);
                    e.CanTrust = true;
                };

                client.Connect();
                client.Disconnect();
            }
            return result;
        }

        internal static string ConvertToSHA256Hex(byte[] hostKey)
        {
            var fingerprint = string.Empty;
            using (SHA256 mySHA256 = SHA256.Create())
            {
                fingerprint = ToHex(mySHA256.ComputeHash(hostKey));
            }
            return fingerprint;
        }

        internal static string ToHex(byte[] bytes)
        {
            StringBuilder result = new(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString("x2"));
            return result.ToString();
        }
    }
}



