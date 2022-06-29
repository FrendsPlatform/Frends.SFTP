using System;
using System.Security.Cryptography;
using Renci.SshNet;
using Renci.SshNet.Common;
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
            Port = 2222,
            Username = _dockerUsername,
            Authentication = AuthenticationType.UsernamePassword,
            Password = _dockerPassword,
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
                client.Create("/listfiles/test" + i + ".txt");
            client.CreateDirectory("/listfiles/subDir");
            for (var i = 1; i <= 3; i++)
                client.Create("/listfiles/subDir/test" + i + ".txt");
            client.Disconnect();
        }
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

    internal static void DeleteTestFiles()
    {
        using (var sftp = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword))
        {
            sftp.Connect();
            sftp.ChangeDirectory("/listfiles");
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
                        sftp.ChangeDirectory("/listfiles");
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
