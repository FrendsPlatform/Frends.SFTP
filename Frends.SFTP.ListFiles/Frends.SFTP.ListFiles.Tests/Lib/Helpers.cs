using System;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Frends.SFTP.ListFiles.Definitions;
using Frends.SFTP.ListFiles.Enums;

namespace Frends.SFTP.ListFiles.Tests;

internal static class Helpers
{
    /// <summary>
    /// Test credentials for docker server.
    /// </summary>
    readonly static string _dockerAddress = "localhost";
    readonly static string _dockerUsername = "foo";
    readonly static string _dockerPassword = "pass";

    internal static Connection GetSftpConnection()
    {
        var connection = new Connection
        {
            ConnectionTimeout = 60,
            Address = _dockerAddress,
            Username = _dockerUsername,
            Password = _dockerPassword,
            Port = 2222,
            Authentication = AuthenticationType.UsernamePassword,
            ServerFingerPrint = null,
        };

        return connection;
    }

    internal static void GenerateDummyFiles()
    {
        using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
        {
            client.Connect();
            for (var i = 1; i <= 3; i++)
                client.Create("/upload/test" + i + ".txt");
            client.CreateDirectory("/upload/subDir");
            for (var i = 1; i <= 3; i++)
                client.Create("/upload/subDir/test" + i + ".txt");
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

    internal static void DeleteTestFiles()
    {
        using (var sftp = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
        {
            sftp.Connect();
            sftp.ChangeDirectory("/upload");
            var files = sftp.ListDirectory(".");
            foreach (var file in files)
            {
                if (file.Name != "." && file.Name != "..")
                {
                    if (file.IsDirectory)
                    {
                        sftp.ChangeDirectory(file.FullName);
                        foreach (var f in sftp.ListDirectory("."))
                        {
                            if (f.Name != "." && f.Name != "..")
                            {
                                sftp.DeleteFile(f.Name);
                            }
                        }
                        sftp.ChangeDirectory("/upload");
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

    internal static SshKeyGenerator.SshKeyGenerator GenerateDummySshKey()
    {
        var keyBits = 2048;

        return new SshKeyGenerator.SshKeyGenerator(keyBits);
    }
}
