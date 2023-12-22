﻿using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Frends.SFTP.MoveFile.Definitions;
using Frends.SFTP.MoveFile.Enums;

namespace Frends.SFTP.MoveFile.Tests;

internal static class Helpers
{
    /// <summary>
    /// Test credentials for docker server.
    /// </summary>
    readonly static string _dockerAddress = "localhost";
    readonly static string _dockerUsername = "foo";
    readonly static string _dockerPassword = "pass";
    readonly static string _baseDir = "/upload";

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

    internal static void GenerateDummyFile(string fileName)
    {
        var content = "This is a test file.";
        var path = _baseDir;
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        path = Path.Combine(path, fileName).Replace("\\", "/");
        client.WriteAllText(path, content);
        client.Disconnect();
    }

    internal static bool DestinationFileExists(string dest)
    {
        using var client = new SftpClient(_dockerAddress, 2222, _dockerUsername, _dockerPassword);
        client.Connect();
        var exists = client.Exists(dest);
        client.Disconnect();
        return exists;
    }

    internal static void DeleteSourceFiles(SftpClient client, string dir)
    {
        foreach (var file in client.ListDirectory(dir))
        {
            if (file.Name != "." && file.Name != "..")
            {
                if (file.IsDirectory)
                    DeleteSourceFiles(client, file.FullName);
                else
                    client.DeleteFile(file.FullName);
            }
        }
        if (!dir.Equals("/upload/"))
            client.DeleteDirectory(dir);
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
        var result = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString("x2"));
        return result.ToString();
    }

    internal static SshKeyGenerator.SshKeyGenerator GenerateDummySshKey()
    {
        var keyBits = 2048;

        return new SshKeyGenerator.SshKeyGenerator(keyBits);
    }
}
