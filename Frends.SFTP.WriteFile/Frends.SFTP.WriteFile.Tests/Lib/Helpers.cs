using System;
using System.Security.Cryptography;
using Renci.SshNet;
using Renci.SshNet.Common;
using Frends.SFTP.WriteFile.Definitions;
using Frends.SFTP.WriteFile.Enums;

namespace Frends.SFTP.WriteFile.Tests;

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

    internal static bool DestinationFileExists(string path)
    {
        bool exists;
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        exists = client.Exists(path);
        client.Disconnect();

        return exists;
    }

    internal static string GetDestinationFileContent(string path)
    {
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        var content = client.ReadAllText(path);
        client.Disconnect();
        return content;
    }

    internal static void DeleteDestinationFile(string path)
    {
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        client.DeleteFile(path);
        client.Disconnect();
    }

    internal static void DeleteDestinationFiles()
    {
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        DeleteDirectory(client, "/write");
        client.Disconnect();
    }

    internal static SshKeyGenerator.SshKeyGenerator GenerateDummySshKey()
    {
        var keyBits = 2048;

        return new SshKeyGenerator.SshKeyGenerator(keyBits);
    }
}
