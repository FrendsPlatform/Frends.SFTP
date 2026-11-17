namespace Frends.SFTP.WriteFile.Enums;

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
    /// ANSI encoding.
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
    /// Unicode encoding.
    /// </summary>
    Unicode,

    /// <summary>
    /// Use a custom encoding name.
    /// </summary>
    Other,
}
