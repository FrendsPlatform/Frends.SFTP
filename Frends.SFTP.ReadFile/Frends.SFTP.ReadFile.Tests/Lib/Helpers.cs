using System;
using System.Security.Cryptography;
using System.Text;
using Frends.SFTP.ReadFile.Definitions;
using Frends.SFTP.ReadFile.Enums;
using Renci.SshNet;
using Renci.SshNet.Security;

namespace Frends.SFTP.ReadFile.Tests.Lib;

internal static class Helpers
{
    /// <summary>
    /// Test credentials for docker server.
    /// </summary>
    private static readonly string DockerAddress = "localhost";
    private static readonly string DockerUsername = "foo";
    private static readonly string DockerPassword = "pass";

    internal static Connection GetSftpConnection()
    {
        var connection = new Connection
        {
            ConnectionTimeout = 60,
            Address = DockerAddress,
            Port = 2222,
            Username = DockerUsername,
            Authentication = AuthenticationType.UsernamePassword,
            Password = DockerPassword,
            ServerFingerPrint = null,
            BufferSize = 32,
        };

        return connection;
    }

    internal static void GenerateDummyFile(string path, string content)
    {
        using var client = new SftpClient(DockerAddress, 2222, DockerUsername, DockerPassword);
        client.Connect();
        client.WriteAllText(path, content);
        client.Disconnect();
    }

    internal static void OverrideDummyFile(string path, string content)
    {
        using var client = new SftpClient(DockerAddress, 2222, DockerUsername, DockerPassword);
        client.Connect();
        if (client.Exists(path))
            client.DeleteFile(path);
        client.WriteAllText(path, content);
        client.Disconnect();
    }

    internal static Tuple<string, string, byte[]> GetServerFingerPrintsAndHostKey()
    {
        Tuple<string, string, byte[]> result = null;
        using (var client = new SftpClient(DockerAddress, 2222, DockerUsername, DockerPassword))
        {
            client.ConnectionInfo.HostKeyAlgorithms.Clear();
            client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-rsa", (data) =>
            {
                var sshKeyData = new SshKeyData(data);
                return new KeyHostAlgorithm("ssh-rsa", new RsaKey(sshKeyData));
            });

            client.HostKeyReceived += (sender, e) =>
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
        var result = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString("x2"));
        return result.ToString();
    }

    internal static void DeleteSourceFile(string path)
    {
        using var client = new SftpClient(DockerAddress, 2222, DockerUsername, DockerPassword);
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
