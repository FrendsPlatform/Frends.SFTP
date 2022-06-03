using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using Renci.SshNet;
using Renci.SshNet.Common;
using Frends.SFTP.DownloadFiles.Definitions;

namespace Frends.SFTP.DownloadFiles.Tests
{
    internal static class Helpers
    {
        /// <summary>
        /// Test credentials for docker server.
        /// </summary>
        private static string _dockerAddress = "localhost";
        private static string _dockerUsername = "foo";
        private static string _dockerPassword = "pass";

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
                ServerFingerPrint = null,
                BufferSize = 32
            };

            return connection;
        }

        internal static string GetServerFingerprintAsSHA256String()
        {
            var fingerprint = "";
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                {
                    // First try with SHA256 typed fingerprint
                    using (SHA256 mySHA256 = SHA256.Create())
                    {
                        fingerprint = Convert.ToBase64String(mySHA256.ComputeHash(e.HostKey));
                    }
                };
            }
            return fingerprint;
        }

        internal static string GetServerFingerprintAsMD5String()
        {
            var fingerprint = "";
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                {
                    fingerprint = BitConverter.ToString(e.FingerPrint).Replace("-", ":");
                };
            }
            return fingerprint;
        }

        internal static void DeleteDirectory(SftpClient client, string dir)
        {

            foreach (var file in client.ListDirectory(dir))
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
            if (client.Exists(dir))
                client.DeleteDirectory(dir);
        }

        internal static void UploadTestFiles(List<string> paths, string destination)
        {
            using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                client.Connect();
                CreateSourceDirectories(client, destination);
                client.ChangeDirectory(destination);
                foreach (var path in paths)
                {
                    using (var fs = File.OpenRead(path))
                    {
                        client.UploadFile(fs, Path.GetFileName(path));
                    }

                }
                client.Disconnect();
            }
        }

        internal static void CreateSourceDirectories(SftpClient client, string path)
        {

            foreach (string dir in path.Split('/'))
            {
                if (dir.Length <= 0)
                    continue;
                if (!client.Exists(dir))
                {
                    client.CreateDirectory(dir);
                }
                client.ChangeDirectory(dir);
            }
            client.ChangeDirectory("/");
        }
    }
}
