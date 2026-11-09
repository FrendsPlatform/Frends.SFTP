namespace Frends.SFTP.ListFiles.Enums;

/// <summary>
/// Enumeration of file encoding options.
/// </summary>
public enum FileEncoding
{
    /// <summary>
    /// UTF-8 encoding.
    /// </summary>
    UTF8,

    /// <summary>
    /// System-default ANSI encoding.
    /// </summary>
    ANSI,

    /// <summary>
    /// ASCII encoding.
    /// </summary>
    ASCII,

    /// <summary>
    /// Windows-1252 encoding.
    /// </summary>
    WINDOWS1252,

    /// <summary>
    /// Other enables users to add other encoding options as string.
    /// </summary>
    Other,
}
