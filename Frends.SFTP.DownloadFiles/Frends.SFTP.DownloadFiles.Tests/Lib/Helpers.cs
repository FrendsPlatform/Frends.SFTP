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

namespace Frends.SFTP.DownloadFiles.Tests;

internal static class Helpers
{
    /// <summary>
    /// Test credentials for docker server.
    /// </summary>
    readonly static string _dockerAddress = "localhost";
    readonly static string _dockerUsername = "foo";
    readonly static string _dockerPassword = "pass";
    readonly static string _baseDir = "/upload/";

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
            BufferSize = 32
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

    internal static void UploadTestFiles(List<string> paths, string destination, string to = null)
    {
        using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
        {
            client.Connect();
            CreateSourceDirectories(client, destination);
            client.ChangeDirectory(destination);
            if (!string.IsNullOrEmpty(to)) client.CreateDirectory(to);
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

    internal static string ConvertToSHA1(byte[] hostKey)
    {
        var fingerprint = "";
        using (var sha1 = SHA1.Create())
        {
            var hash = sha1.ComputeHash(hostKey);
            fingerprint = string.Concat(hash.Select(b => b.ToString("x2")));
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

