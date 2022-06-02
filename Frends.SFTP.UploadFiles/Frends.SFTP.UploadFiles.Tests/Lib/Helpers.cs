using System;
using System.Security.Cryptography;
using Renci.SshNet;
using Renci.SshNet.Common;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests
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
                UserName = _dockerUsername,
                Password = _dockerPassword,
                Port = 2222,
                Authentication = AuthenticationType.UsernamePassword,
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

        internal static long GetTransferredFileContent(string fullPath)
        {
            long size;
            using (var sftp = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                sftp.Connect();

                size = sftp.Get(fullPath).Length;
                sftp.Disconnect();
                sftp.Dispose();
            }

            return size;
        }

        internal static bool CheckFileExistsInDestination(string fullpath)
        {
            using (var sftp = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
            {
                var exists = true;
                sftp.Connect();
                if (!sftp.Exists(fullpath))
                    exists = false;
                sftp.Disconnect();
                return exists;
            }
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
    }
}
