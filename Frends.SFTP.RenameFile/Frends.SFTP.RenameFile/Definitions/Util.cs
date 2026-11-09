using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Frends.SFTP.RenameFile.Enums;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;

namespace Frends.SFTP.RenameFile.Definitions;

/// <summary>
/// Helper methods for file modifications
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
            result.Append(bytes[i].ToString("x2"));

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
        {
            return false;
        }
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
            if (!input.EndsWith("="))
                input += '=';
            Convert.FromBase64String(input);

            return true;
        }
        catch
        {
            return false;
        }
    }

    internal static void AddServerFingerprintCheck(SftpClient client, string expectedServerFingerprint)
    {
        var userResultMessage = string.Empty;
        var md5ServerFingerprint = string.Empty;
        var shaServerFingerprint = string.Empty;

        client.HostKeyReceived += (_, e) =>
        {
            try
            {
                md5ServerFingerprint = e.FingerPrintMD5;
                shaServerFingerprint = e.FingerPrintSHA256;

                if (!string.IsNullOrEmpty(expectedServerFingerprint))
                {
                    if (IsMD5(expectedServerFingerprint.Replace(":", string.Empty).Replace("-", string.Empty)))
                    {
                        if (!expectedServerFingerprint.Contains(':'))
                        {
                            e.CanTrust = string.Equals(
                                expectedServerFingerprint.Replace("-", string.Empty),
                                md5ServerFingerprint.Replace(":", string.Empty).Replace("-", string.Empty),
                                StringComparison.CurrentCultureIgnoreCase);
                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{md5ServerFingerprint}'.";
                            }
                        }
                        else
                        {
                            e.CanTrust =
                                e.FingerPrint.SequenceEqual(ConvertFingerprintToByteArray(expectedServerFingerprint));
                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{md5ServerFingerprint}'.";
                            }
                        }
                    }
                    else if (IsSha256(expectedServerFingerprint))
                    {
                        if (TryConvertHexStringToHex(expectedServerFingerprint))
                        {
                            using (var mySHA256 = SHA256.Create())
                            {
                                shaServerFingerprint = ToHex(mySHA256.ComputeHash(e.HostKey));
                            }

                            e.CanTrust = string.Equals(
                                shaServerFingerprint,
                                expectedServerFingerprint,
                                StringComparison.OrdinalIgnoreCase);
                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{shaServerFingerprint}'.";
                            }
                        }
                        else
                        {
                            e.CanTrust = string.Equals(
                                             shaServerFingerprint,
                                             expectedServerFingerprint,
                                             StringComparison.OrdinalIgnoreCase)
                                         ||
                                         string.Equals(
                                             shaServerFingerprint.Replace("=", string.Empty),
                                             expectedServerFingerprint,
                                             StringComparison.OrdinalIgnoreCase)
                                ;
                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{shaServerFingerprint}'.";
                            }
                        }
                    }
                    else
                    {
                        userResultMessage = "Expected server fingerprint was given in unsupported format.";
                        e.CanTrust = false;
                    }
                }

                if (!e.CanTrust) throw new ArgumentException(userResultMessage);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error when checking the server fingerprint: {ex.Message}", ex);
            }
        };
    }

    internal static void ForceHostKeyAlgorithm(SftpClient client, HostKeyAlgorithms algorithm)
    {
        client.ConnectionInfo.HostKeyAlgorithms.Clear();

        switch (algorithm)
        {
            case HostKeyAlgorithms.RSA:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-rsa", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);

                    return new KeyHostAlgorithm("ssh-rsa", new RsaKey(sshKeyData));
                });

                break;
            case HostKeyAlgorithms.Ed25519:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ssh-ed25519", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);

                    return new KeyHostAlgorithm("ssh-ed25519", new ED25519Key(sshKeyData));
                });

                break;
            case HostKeyAlgorithms.Nistp256:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp256", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);

                    return new KeyHostAlgorithm("ecdsa-sha2-nistp256", new EcdsaKey(sshKeyData));
                });

                break;
            case HostKeyAlgorithms.Nistp384:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp384", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);

                    return new KeyHostAlgorithm("ecdsa-sha2-nistp384", new EcdsaKey(sshKeyData));
                });

                break;
            case HostKeyAlgorithms.Nistp521:
                client.ConnectionInfo.HostKeyAlgorithms.Add("ecdsa-sha2-nistp521", (data) =>
                {
                    var sshKeyData = new SshKeyData(data);

                    return new KeyHostAlgorithm("ecdsa-sha2-nistp521", new EcdsaKey(sshKeyData));
                });

                break;
        }

        return;
    }
}
