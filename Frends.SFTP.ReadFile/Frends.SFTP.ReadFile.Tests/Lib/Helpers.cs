using System;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Frends.SFTP.ReadFile.Definitions;
using Frends.SFTP.ReadFile.Enums;

namespace Frends.SFTP.ReadFile.Tests;

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
            Port = 2222,
            Username = _dockerUsername,
            Authentication = AuthenticationType.UsernamePassword,
            Password = _dockerPassword,
            ServerFingerPrint = null,
            BufferSize = 32
        };

        return connection;
    }

    internal static void GenerateDummyFile(string path, string content)
    {
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        client.WriteAllText(path, content);
        client.Disconnect();
    }

    internal static void OverrideDummyFile(string path, string content)
    {
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        if (client.Exists(path))
            client.DeleteFile(path);
        client.WriteAllText(path, content);
        client.Disconnect();
    }

    internal static Tuple<string, string, byte[]> GetServerFingerPrintsAndHostKey()
    {
        Tuple<string, string, byte[]> result = null;
        using (var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
        {
            client.ConnectionInfo.HostKeyAlgorithms.Clear();
            client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-rsa", (data) => { return new KeyHostAlgorithm("ssh-rsa", new RsaKey(), data); });

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
        var fingerprint = "";
        using (SHA256 mySHA256 = SHA256.Create())
        {
            fingerprint = ToHex(mySHA256.ComputeHash(hostKey));
        }
        return fingerprint;
    }

    internal static string ToHex(byte[] bytes)
    {
        var result = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString("x2"));
        return result.ToString();
    }

    internal static void DeleteSourceFile(string path)
    {
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        client.DeleteFile(path);
        client.Disconnect();
    }

    internal static SshKeyGenerator.SshKeyGenerator GenerateDummySshKey()
    {
        var keyBits = 2048;

        return new SshKeyGenerator.SshKeyGenerator(keyBits);
    }
}
