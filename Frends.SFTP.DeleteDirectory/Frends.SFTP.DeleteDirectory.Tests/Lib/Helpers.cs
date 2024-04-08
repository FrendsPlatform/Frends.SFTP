﻿namespace Frends.SFTP.DeleteDirectory.Tests;

using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Frends.SFTP.DeleteDirectory.Definitions;
using Frends.SFTP.DeleteDirectory.Enums;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;

internal static class Helpers
{
    /// <summary>
    /// Test credentials for docker server.
    /// </summary>
    private static readonly string _dockerAddress = Environment.GetEnvironmentVariable("SFTP_DockerAddress"); // localhost
    private static readonly string _dockerUsername = Environment.GetEnvironmentVariable("SFTP_DockerUsername"); // "foo"
    private static readonly string _dockerPassword = Environment.GetEnvironmentVariable("SFTP_DockerPassword"); // "pass"

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
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        for (var i = 1; i <= 3; i++)
            client.Create("/upload/test" + i + ".txt");
        client.CreateDirectory("/upload/subDir");
        for (var i = 1; i <= 3; i++)
            client.Create("/upload/subDir/test" + i + ".txt");
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

    internal static void DeleteTestFiles()
    {
        using var sftp = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        sftp.Connect();
        sftp.ChangeDirectory("/upload");
        var files = sftp.ListDirectory(".");
        var validFiles = files.Where(file => file.Name != "." && file.Name != "..");

        foreach (var file in validFiles)
        {
            if (file.IsDirectory)
            {
                sftp.ChangeDirectory(file.FullName);
                var directoryFiles = sftp.ListDirectory(".").Where(f => f.Name != "." && f.Name != "..");

                foreach (var f in directoryFiles)
                    sftp.DeleteFile(f.Name);

                sftp.ChangeDirectory("/upload");
                sftp.DeleteDirectory(file.FullName);
            }
            else
            {
                sftp.DeleteFile(file.FullName);
            }
        }

        sftp.Disconnect();
    }
}