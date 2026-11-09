using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Frends.SFTP.ListFiles.Enums;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;

namespace Frends.SFTP.ListFiles.Definitions;

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
        var result = new StringBuilder(bytes.Length * 2);
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

    /// <summary>
    /// Get encoding for the file name to be transferred.
    /// </summary>
    /// <param name="encoding">The selected file encoding.</param>
    /// <param name="encodingString">The encoding name when <paramref name="encoding"/> is <see cref="FileEncoding.Other"/>.</param>
    /// <param name="enableBom">Whether to include the byte-order mark for UTF-8.</param>
    /// <returns>The encoding used to read file names.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The selected encoding is not supported.</exception>
    internal static Encoding GetEncoding(FileEncoding encoding, string encodingString, bool enableBom)
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
                return GetOtherEncoding(encodingString);
            default:
                throw new ArgumentOutOfRangeException($"Unknown Encoding type: '{encoding}'.");
        }
    }

    internal static Encoding GetOtherEncoding(string encodingString)
    {
        if (string.IsNullOrWhiteSpace(encodingString))
            throw new ArgumentException("Encoding name was not given.");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        try
        {
            return Encoding.GetEncoding(encodingString);
        }
        catch (ArgumentException)
        {
            throw new ArgumentException($"Unknown encoding: '{encodingString}'.");
        }
    }

    internal static void AddServerFingerprintCheck(SftpClient client, string expectedServerFingerprint)
    {
        var userResultMessage = string.Empty;
        var mD5serverFingerprint = string.Empty;
        var sHAServerFingerprint = string.Empty;

        client.HostKeyReceived += (_, e) =>
        {
            try
            {
                mD5serverFingerprint = e.FingerPrintMD5;
                sHAServerFingerprint = e.FingerPrintSHA256;

                if (!string.IsNullOrEmpty(expectedServerFingerprint))
                {
                    if (IsMD5(expectedServerFingerprint.Replace(":", string.Empty).Replace("-", string.Empty)))
                    {
                        if (!expectedServerFingerprint.Contains(':'))
                        {
                            e.CanTrust = expectedServerFingerprint.ToLower() ==
                                         mD5serverFingerprint.Replace(":", string.Empty).ToLower();
                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{mD5serverFingerprint}'.";
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
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{mD5serverFingerprint}'.";
                            }
                        }
                    }
                    else if (IsSha256(expectedServerFingerprint))
                    {
                        if (TryConvertHexStringToHex(expectedServerFingerprint))
                        {
                            using (var mySHA256 = SHA256.Create())
                            {
                                sHAServerFingerprint = ToHex(mySHA256.ComputeHash(e.HostKey));
                            }

                            e.CanTrust = sHAServerFingerprint == expectedServerFingerprint;
                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{sHAServerFingerprint}'.";
                            }
                        }
                        else
                        {
                            e.CanTrust = sHAServerFingerprint == expectedServerFingerprint ||
                                          sHAServerFingerprint.Replace("=", string.Empty) == expectedServerFingerprint;
                            if (!e.CanTrust)
                            {
                                userResultMessage =
                                    $"Can't trust SFTP server. The server fingerprint does not match. " +
                                    $"Expected fingerprint: '{expectedServerFingerprint}', but was: '{sHAServerFingerprint}'.";
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
            catch (Exception exception)
            {
                throw new ArgumentException(
                    $"Error when checking the server fingerprint: {exception.Message}",
                    exception);
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
