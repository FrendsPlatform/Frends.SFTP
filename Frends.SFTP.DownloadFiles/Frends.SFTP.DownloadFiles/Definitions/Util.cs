using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace Frends.SFTP.DownloadFiles.Definitions;

/// <summary>
/// Helper methods for file modifications
/// </summary>
internal static class Util
{
    internal static string CreateUniqueFileName(string fileExtension)
    {
        return Path.ChangeExtension("frends_" + DateTime.Now.Ticks + Path.GetRandomFileName(), fileExtension);
    }

    /// <summary>
    /// Checks if the file name matches the given file mask. 
    /// The file mask is checked with a kludgey regular expression.
    /// </summary>
    internal static bool FileMatchesMask(string filename, string mask)
    {
        const string regexEscape = "<regex>";
        string pattern;

        //check is pure regex wished to be used for matching
        if (mask.StartsWith(regexEscape))
            //use substring instead of string.replace just in case some has regex like '<regex>//File<regex>' or something else like that
            pattern = mask.Substring(regexEscape.Length);
        else
        {
            pattern = mask.Replace(".", "\\.");
            pattern = pattern.Replace("*", ".*");
            pattern = pattern.Replace("?", ".+");
            pattern = String.Concat("^", pattern, "$");
        }

        return Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase);
    }

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
            {
                arr[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return true;
        } catch { return false; }
    }

    internal static string HexStringToB64String(string input)
    {
        return Convert.ToBase64String(ConvertHexStringToHex(input));
    }

    internal static byte[] ConvertHexStringToHex(string hex)
    {
        var arr = new byte[hex.Length / 2];
        for (var i = 0; i < arr.Length; i++)
        {
            arr[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return arr;
    }

    internal static bool IsMD5(string input)
    {
        if (String.IsNullOrEmpty(input))
        {
            return false;
        }

        return Regex.IsMatch(input, "^[0-9a-fA-F]{32}$");
    }

    internal static bool IsSha1(string input)
    {
        if (String.IsNullOrEmpty(input))
        {
            return false;
        }

        return Regex.IsMatch(input, "^[0-9a-fA-F]{40}$");
    }

    internal static bool IsSha256(string input)
    {
        if (String.IsNullOrEmpty(input))
        {
            return false;
        }

        if (Regex.IsMatch(input, "^[0-9a-fA-F]{64}$"))
            return true;

        try
        {
            if (!input.EndsWith("="))
                input += '=';
            Convert.FromBase64String(input);
            return true;
        } 
        catch { return false; }
    }

    /// <summary>
    /// Get encoding for the file name to be transferred.
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="encodingString"></param>
    /// <param name="enableBom"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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
                return CodePagesEncodingProvider.Instance.GetEncoding("windows-1252");
            case FileEncoding.Other:
                return CodePagesEncodingProvider.Instance.GetEncoding(encodingString);
            default:
                throw new ArgumentOutOfRangeException($"Unknown Encoding type: '{encoding}'.");
        }
    }
}

