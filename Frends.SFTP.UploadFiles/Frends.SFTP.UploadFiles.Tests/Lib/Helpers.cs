using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Frends.SFTP.UploadFiles.Definitions;

namespace Frends.SFTP.UploadFiles.Tests;

internal static class Helpers
{
    /// <summary>
    /// Test credentials for docker server.
    /// </summary>
    readonly static string _dockerAddress = "localhost";
    readonly static string _dockerUsername = "foo";
    readonly static string _dockerPassword = "pass";
    readonly static string _workDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/");

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

    internal static string GetLastWriteTimeFromDestination(string path)
    {
        using (var sftp = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
        {
            sftp.Connect();
            var date = sftp.GetLastWriteTime(path);
            sftp.Disconnect();
            return date.ToString();
        }
    }

    internal static void DeleteDirectory(SftpClient client, string dir)
    {
            
        foreach (var file in client.ListDirectory(dir))
        {
            if ((file.Name != ".") && (file.Name != ".."))
            {
                if (file.IsDirectory)
                    DeleteDirectory(client, file.FullName);
                else
                    client.DeleteFile(file.FullName);
            }
        }
        if (client.Exists(dir))
            client.DeleteDirectory(dir);
    }

    internal static void UploadSingleTestFile(string dir, string pathToFile)
    {
        var path = dir + "/" + Path.GetFileName(pathToFile);
        using (var sftp = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
        {
            sftp.Connect();
            if (!sftp.Exists(dir)) sftp.CreateDirectory(dir);
            using (var fs = File.Open(pathToFile, FileMode.Open))
            {
                sftp.UploadFile(fs, path);
            }
            sftp.Disconnect();
        }
    }

    internal static void CreateDummyFiles(int count)
    {
        var name = "SFTPUploadTestFile";
        var extension = ".txt";
        for (var i = 1; i <= count; i++)
        {
            File.WriteAllText(Path.Combine(_workDir, name + i + extension), "This is a test file.");
        }
    }

    internal static void DeleteDummyFiles()
    {
        foreach (var file in Directory.EnumerateFileSystemEntries(_workDir))
        {
            if (Directory.Exists(file))
                Directory.Delete(file, true);
            if (!file.Contains("LargeTestFile.bin"))
                File.Delete(file);
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

