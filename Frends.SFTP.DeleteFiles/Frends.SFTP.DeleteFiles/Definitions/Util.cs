namespace Frends.SFTP.DeleteFiles.Definitions;

using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Frends.SFTP.DeleteFiles.Enums;

/// <summary>
/// Helper methods for connection builder.
/// </summary>
internal static class Util
{
    internal static byte[] ConvertFingerprintToByteArray(string fingerprint)
    {
        return fingerprint.Split(':').Select(s => Convert.ToByte(s, 16)).ToArray();
    }

    internal static string ToHex(byte[] bytes)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++)
            _ = result.Append(bytes[i].ToString("x2"));
        return result.ToString();
    }

    internal static bool TryConvertHexStringToHex(string hex)
    {
        try
        {
            var arr = new byte[hex.Length / 2];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return true;
        }
        catch
        { return false; }
    }

    internal static bool IsMD5(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return Regex.IsMatch(input, "^[0-9a-fA-F]{32}$");
    }

    internal static bool IsSha256(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        if (Regex.IsMatch(input, "^[0-9a-fA-F]{64}$"))
            return true;

        try
        {
            if (!input.EndsWith('='))
                input += '=';
            Convert.FromBase64String(input);
            return true;
        }
        catch { return false; }
    }

    internal static string CheckServerFingerprint(SftpClient client, string expectedServerFingerprint)
    {
        var userResultMessage = string.Empty;

        client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
        {
            if (IsMD5(expectedServerFingerprint.Replace(":", string.Empty).Replace("-", string.Empty)))
            {
                if (!expectedServerFingerprint.Contains(':'))
                {
                    var serverFingerprint = BitConverter.ToString(e.FingerPrint).Replace("-", string.Empty).Replace(":", string.Empty);

                    e.CanTrust = expectedServerFingerprint.ToLower() == serverFingerprint.ToLower();
                    if (!e.CanTrust)
                    {
                        userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{serverFingerprint}'.";
                    }
                }
                else
                {
                    var serverFingerprint = BitConverter.ToString(e.FingerPrint).Replace('-', ':');

                    e.CanTrust = e.FingerPrint.SequenceEqual(ConvertFingerprintToByteArray(expectedServerFingerprint));
                    if (!e.CanTrust)
                    {
                        userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{serverFingerprint}'.";
                    }
                }
            }
            else if (IsSha256(expectedServerFingerprint))
            {
                if (TryConvertHexStringToHex(expectedServerFingerprint))
                {
                    using (SHA256 mySHA256 = SHA256.Create())
                    {
                        var sha256Fingerprint = ToHex(mySHA256.ComputeHash(e.HostKey));

                        e.CanTrust = sha256Fingerprint == expectedServerFingerprint;
                        if (!e.CanTrust)
                        {
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{sha256Fingerprint}'.";
                        }
                    }
                }
                else
                {
                    using (SHA256 mySHA256 = SHA256.Create())
                    {
                        var sha256Fingerprint = Convert.ToBase64String(mySHA256.ComputeHash(e.HostKey));
                        e.CanTrust = sha256Fingerprint == expectedServerFingerprint || sha256Fingerprint.Replace("=", string.Empty) == expectedServerFingerprint;
                        if (!e.CanTrust)
                        {
                            userResultMessage = $"Can't trust SFTP server. The server fingerprint does not match. " +
                                                $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{sha256Fingerprint}'.";
                        }
                    }
                }
            }
            else
            {
                userResultMessage = "Expected server fingerprint was given in unsupported format.";
                e.CanTrust = false;
            }
        };

        return userResultMessage;
    }

    internal static void ForceHostKeyAlgorithm(SftpClient client, HostKeyAlgorithms algorithm)
    {
        client.ConnectionInfo.HostKeyAlgorithms.Clear();

        switch (algorithm)
        {
            case HostKeyAlgorithms.RSA:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-rsa", (data) => { return new KeyHostAlgorithm("ssh-rsa", new RsaKey(), data); });
                break;
            case HostKeyAlgorithms.Ed25519:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-ed25519", (data) => { return new KeyHostAlgorithm("ssh-ed25519", new ED25519Key(), data); });
                break;
            case HostKeyAlgorithms.DSS:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-dss", (data) => { return new KeyHostAlgorithm("ssh-dss", new DsaKey(), data); });
                break;
            case HostKeyAlgorithms.Nistp256:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp256", (data) => { return new KeyHostAlgorithm("ecdsa-sha2-nistp256", new EcdsaKey(), data); });
                break;
            case HostKeyAlgorithms.Nistp384:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp384", (data) => { return new KeyHostAlgorithm("ecdsa-sha2-nistp384", new EcdsaKey(), data); });
                break;
            case HostKeyAlgorithms.Nistp521:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp521", (data) => { return new KeyHostAlgorithm("ecdsa-sha2-nistp521", new EcdsaKey(), data); });
                break;
        }
    }

    internal static Encoding GetEncoding(FileEncoding encoding, bool enableBom, string encodingString = null)
    {
        switch (encoding)
        {
            case FileEncoding.UTF8:
                return enableBom ? new UTF8Encoding(true) : new UTF8Encoding(false);
            case FileEncoding.ASCII:
                return new ASCIIEncoding();
            case FileEncoding.ANSI:
                return Encoding.Default;
            case FileEncoding.WINDOWS1252:
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding("windows-1252");
            case FileEncoding.Other:
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var e = Encoding.GetEncoding(encodingString);
                if (e == null) throw new ArgumentException($"Encoding string {encodingString} is not a valid code page name.");
                return e;
            default:
                throw new ArgumentOutOfRangeException($"Unknown Encoding type: '{encoding}'.");
        }
    }
}