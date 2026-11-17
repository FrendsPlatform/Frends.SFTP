using System;
using System.Security.Cryptography;
using System.Text;
using Frends.SFTP.ListFiles.Definitions;
using Frends.SFTP.ListFiles.Enums;
using Renci.SshNet;
using Renci.SshNet.Security;

namespace Frends.SFTP.ListFiles.Tests.Lib;

internal static class Helpers
{
    /// <summary>
    /// Test credentials for docker server.
    /// </summary>
    private const string DockerAddress = "localhost";
    private const string DockerUsername = "foo";
    private const string DockerPassword = "pass";

    internal static Connection GetSftpConnection()
    {
        var connection = new Connection
        {
            ConnectionTimeout = 60,
            Address = DockerAddress,
            Username = DockerUsername,
            Password = DockerPassword,
            Port = 2222,
            Authentication = AuthenticationType.UsernamePassword,
            ServerFingerPrint = null,
        };

        return connection;
    }

    internal static void GenerateDummyFiles()
    {
        using (var client = new SftpClient(DockerAddress, 2222, DockerUsername, DockerPassword))
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
        StringBuilder result = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString("x2"));
        return result.ToString();
    }

    internal static void DeleteTestFiles()
    {
        using (var sftp = new SftpClient(DockerAddress, 2222, DockerUsername, DockerPassword))
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
