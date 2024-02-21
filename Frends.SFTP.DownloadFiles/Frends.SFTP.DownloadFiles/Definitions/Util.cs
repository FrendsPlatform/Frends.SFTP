namespace Frends.SFTP.DownloadFiles.Definitions;

using System.Text.RegularExpressions;
using System.Text;

/// <summary>
/// Helper methods for file modifications
/// </summary>
internal static class Util
{
    internal static string CreateUniqueFileName(string fileExtension)
    {
        return Path.ChangeExtension("frends_" + DateTime.Now.Ticks + Path.GetRandomFileName(), fileExtension);
    }

    internal static bool FileMatchesMask(string filename, string mask)
    {
        const string regexEscape = "<regex>";
        string pattern;

        // check is pure regex wished to be used for matching
        if (mask.StartsWith(regexEscape))
        {
            // use substring instead of string.replace just in case some has regex like '<regex>//File<regex>' or something else like that
            pattern = mask.Substring(regexEscape.Length);
        }
        else
        {
            pattern = mask.Replace(".", "\\.");
            pattern = pattern.Replace("*", ".*");
            pattern = pattern.Replace("?", ".+");
            pattern = string.Concat("^", pattern, "$");
        }

        return Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase);
    }

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
            {
                arr[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

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
        {
            return false;
        }

        return Regex.IsMatch(input, "^[0-9a-fA-F]{32}$");
    }

    internal static bool IsSha256(string input)
    {
        if (string.IsNullOrEmpty(input))
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
        catch
        {
            return false;
        }
    }

    internal static Encoding GetEncoding(FileEncoding encoding, string encodingString, bool enableBom)
    {
        return encoding switch
        {
            FileEncoding.UTF8 => enableBom ? new UTF8Encoding(true) : new UTF8Encoding(false),
            FileEncoding.ASCII => new ASCIIEncoding(),
            FileEncoding.ANSI => Encoding.Default,
            FileEncoding.WINDOWS1252 => CodePagesEncodingProvider.Instance.GetEncoding("windows-1252"),
            FileEncoding.Other => CodePagesEncodingProvider.Instance.GetEncoding(encodingString),
            _ => throw new ArgumentOutOfRangeException($"Unknown Encoding type: '{encoding}'."),
        };
    }
}