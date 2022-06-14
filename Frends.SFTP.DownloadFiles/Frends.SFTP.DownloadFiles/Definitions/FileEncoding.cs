// Pragma for self-explanatory enum attributes.
#pragma warning disable 1591

namespace Frends.SFTP.DownloadFiles.Definitions;

/// <summary>
/// Enumeration of file encoding options.
/// </summary>
public enum FileEncoding
{
    UTF8,
    UTF16,
    ANSI,
    ASCII,
    WINDOWS1252,
    Unicode,
    /// <summary>
    /// Other enables users to add other encoding options as string.
    /// </summary>
    Other
}

