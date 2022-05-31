﻿using System.Text.RegularExpressions;

namespace Frends.SFTP.UploadFiles.Definitions
{
    /// <summary>
    /// Helper methods for file modifications
    /// </summary>
    internal static class Util
    {
        /// <summary>
        /// Creates unique filename.
        /// </summary>
        /// <param name="toDir"></param>        
        /// <returns>return full path to the unique file (toDir + unique filename)</returns>
        internal static string CreateUniqueFile(string toDir)
        {

            if (!toDir.EndsWith("/") && !toDir.EndsWith("\\"))
            {
                toDir = toDir + "\\";
            }

            return Path.GetFullPath(toDir + (DateTime.Now.Ticks + Path.GetRandomFileName()));
        }
        /// <summary>
        /// Creates unique file name
        /// </summary>        
        /// <returns>return unique file name</returns>
        internal static string CreateUniqueFileName()
        {
            return Path.ChangeExtension("frends_" + DateTime.Now.Ticks + Path.GetRandomFileName(), "8CO");
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

        /// <summary>
        /// Converts given fingerprint into ByteArray.
        /// </summary>
        /// <param name="fingerprint"></param>
        /// <returns></returns>
        internal static byte[] ConvertFingerprintToByteArray(string fingerprint)
        {
            return fingerprint.Split(':').Select(s => Convert.ToByte(s, 16)).ToArray();
        }
    }
}
