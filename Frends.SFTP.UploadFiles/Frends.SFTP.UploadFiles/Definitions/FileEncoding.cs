// Pragma is for self-explanatory enum attributes.
#pragma warning disable 1591

namespace Frends.SFTP.UploadFiles.Definitions;

/// <summary>
/// Encodings used in the file name and file content encoding.
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

